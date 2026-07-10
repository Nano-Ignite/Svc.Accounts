using DynamicExpression.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nano.App.Api.Annotations;
using Nano.App.Api.Controllers;
using Nano.Common.Consts;
using Nano.Data.Abstractions;
using Nano.Data.Abstractions.Identity;
using Nano.Eventing.Abstractions;
using Nano.Storage.Abstractions;
using Svc.Accounts.Models.Criterias;
using Svc.Accounts.Models.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Lib.Images.Extensions;
using Nano.Common.Extensions;
using Svc.Accounts.Consts;
using Svc.Accounts.Models.Api.Requests.Enums;

namespace Svc.Accounts.Controllers;

/// <inheritdoc />
public class UsersController(ILogger<UsersController> logger, IRepository repository, IEventing eventing, IIdentityRepository identityRepository)
    : BaseEntityUserController<User, UserQueryCriteria>(logger, repository, eventing, identityRepository)
{
    private const string PROFILE_PICTURE_PREFIX = "profile-picture";

    private readonly IPathProvider pathProvider = null!;

    /// <inheritdoc />
    public UsersController(ILogger<UsersController> logger, IRepository repository, IEventing eventing, IIdentityRepository identityRepository, IPathProvider pathProvider)
        : this(logger, repository, eventing, identityRepository) 
    {
        this.pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
    }

    // BUG: Remove "Profile" naming

    /// <summary>
    /// Anonymously get a yser by email.
    /// </summary>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="cancellationToken">The token used when request is cancelled.</param>
    /// <returns>The user.</returns>
    /// <response code="200">OK.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Error occurred.</response>
    [HttpGet]
    [Route("details/email")]
    [AllowAnonymous]
    [ResponseCache(NoStore = true)]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType(typeof(User), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetUserByEmailAsync([FromQuery][Required][EmailAddress] string emailAddress, CancellationToken cancellationToken = default)
    {
        var user = await this.Repository
            .GetFirstAsync<User>(x => x.IdentityUser.Email == emailAddress, new Ordering(), cancellationToken);

        if (user == null)
        {
            return this.NotFound();
        }

        return this.Ok(user);
    }

    /// <summary>
    /// Get Profile Picture.
    /// </summary>
    /// <param name="id">The user id.</param>
    /// <param name="type">THe image type.</param>
    /// <param name="cancellationToken">The token used when request is cancelled.</param>
    /// <returns>The profile picture.</returns>
    /// <response code="200">OK.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Error occurred.</response>
    [HttpGet]
    [Route("profile-picture/{type}/{id:guid}")]
    [Produces(HttpContentType.JPEG, HttpContentType.PNG)]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetProfilePictureAsync([FromRoute][Required] Guid id, [FromRoute][Required] ImageType type, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await this.Repository
                .GetAsync<User>(id, cancellationToken);

            if (user?.ProfilePictureExtension == null)
            {
                return this.NotFound();
            }

            var filename = this.GetProfilePictureFilename(user, type);
            var path = Path.Combine(this.pathProvider.Root, filename);
            var bytes = await System.IO.File.ReadAllBytesAsync(path, cancellationToken);
            var extension = Path.GetExtension(path);

            var httpContentType = extension
                .GetHttpContentType();

            return this.File(bytes, httpContentType, filename);
        }
        catch (Exception ex)
        {
            this.Logger
                .LogError(ex, ex.Message);

            return this.NotFound();
        }
    }

    /// <summary>
    /// Add Profile Picture.
    /// </summary>
    /// <param name="id">The user id.</param>
    /// <param name="file">The file.</param>
    /// <param name="cancellationToken">The token used when request is cancelled.</param>
    /// <returns>Void.</returns>
    /// <response code="200">OK.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Error occurred.</response>
    [HttpPost]
    [Route("profile-picture/add/{id:guid}")]
    [RequestSizeLimit(1024 * 1024 * 1024)]
    [Consumes(HttpContentType.FORM)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> AddProfilePictureAsync([FromRoute][Required] Guid id, [Required][FileExtensionValidation(FileExtensions.JPG, FileExtensions.JPEG, FileExtensions.PNG)] IFormFile file, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName);

        var user = await this.Repository
            .GetAsync<User>(id, cancellationToken);

        if (user == null)
        {
            return this.NotFound();
        }

        user.ProfilePictureExtension = extension;

        user = await this.Repository
            .UpdateAsync(user, cancellationToken);

        await this.SaveProfilePictureAsync(file, user, ImageType.Thunbnail, cancellationToken);
        await this.SaveProfilePictureAsync(file, user, ImageType.Full, cancellationToken);

        return this.Ok();
    }

    /// <summary>
    /// Remove Profile Picture.
    /// </summary>
    /// <param name="id">The user id.</param>
    /// <param name="cancellationToken">The token used when request is cancelled.</param>
    /// <returns>Void.</returns>
    /// <response code="200">OK.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Error occurred.</response>
    [HttpDelete]
    [Route("profile-picture/remove/{id:guid}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> RemoveProfilePictureAsync([FromRoute][Required] Guid id, CancellationToken cancellationToken = default)
    {
        var user = await this.Repository
            .GetAsync<User>(id, cancellationToken);

        if (user?.ProfilePictureExtension == null)
        {
            return this.NotFound();
        }

        this.DeleteProfilePictureAsync(user, ImageType.Thunbnail);
        this.DeleteProfilePictureAsync(user, ImageType.Full);

        user.ProfilePictureExtension = null;

        await this.Repository
            .UpdateAsync(user, cancellationToken);

        return this.Ok();
    }


    private string GetProfilePictureFilename(User user, ImageType type)
    {
        ArgumentNullException.ThrowIfNull(user);

        return $"{PROFILE_PICTURE_PREFIX}-{type.ToString().ToLower()}-{user.Id}{user.ProfilePictureExtension}";
    }
    private async Task SaveProfilePictureAsync(IFormFile file, User user, ImageType type, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(user);

        await using var image = file
            .OpenReadStream();

        var size = type == ImageType.Thunbnail
            ? ImageSizes.THUMBNAIL
            : ImageSizes.FULL;

        await using var resizeImage = image
            .ResizeImage(size, size);

        var filename = this.GetProfilePictureFilename(user, type);

        var path = Path.Combine(this.pathProvider.Root, filename);

        await resizeImage
            .SaveFileAsync(path, cancellationToken);
    }
    private void DeleteProfilePictureAsync(User user, ImageType type)
    {
        ArgumentNullException.ThrowIfNull(user);

        var filename = this.GetProfilePictureFilename(user, type);
        var path = Path.Combine(this.pathProvider.Root, filename);

        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }
    }
}