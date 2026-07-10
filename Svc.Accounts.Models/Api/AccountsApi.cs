using Microsoft.AspNetCore.Http;
using Svc.Accounts.Models.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Nano.App.ApiClient;
using Nano.App.ApiClient.Models;
using Svc.Accounts.Models.Api.Requests;
using Svc.Accounts.Models.Api.Requests.Enums;

namespace Svc.Accounts.Models.Api;

/// <inheritdoc />
public class AccountsApi : BaseIdentityApiClient<User>
{
    /// <inheritdoc />
    public AccountsApi(ApiClient apiClient)
        : base(apiClient)
    {
    }

    /// <summary>
    /// Get User Async.
    /// </summary>
    /// <param name="emailAddress">The email address.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The instance of <see cref="User"/>.</returns>
    public virtual Task<User?> GetUserAsync(string emailAddress, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync<UserDetailsByEmailRequest, User>(new UserDetailsByEmailRequest
        {
            EmailAddress = emailAddress
        }, cancellationToken);
    }

    /// <summary>
    /// Get User Picture.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="type">The image type.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The user picture.</returns>
    public virtual Task<NamedStream?> GetUserPictureAsync(Guid userId, ImageType type, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync<GetUserPictureRequest, NamedStream>(new GetUserPictureRequest
        {
            Id = userId,
            Type = type
        }, cancellationToken);
    }

    /// <summary>
    /// Add User Picture.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="file">The picture file.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Void.</returns>
    public virtual Task AddUserPictureAsync(Guid userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(new AddUserPictureRequest
        {
            Id = userId,
            File = file
        }, cancellationToken);
    }

    /// <summary>
    /// Remove User Picture.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>Void.</returns>
    public virtual Task RemoveUserPictureAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return this.InvokeAsync(new RemoveUserPictureRequest
        {
            Id = userId
        }, cancellationToken);
    }
}