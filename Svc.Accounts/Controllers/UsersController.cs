using DynamicExpression.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nano.App.Api.Controllers;
using Nano.Common.Consts;
using Nano.Data.Abstractions;
using Nano.Data.Abstractions.Identity;
using Nano.Eventing.Abstractions;
using Svc.Accounts.Models.Criterias;
using Svc.Accounts.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Svc.Accounts.Controllers;

/// <inheritdoc />
public class UsersController(ILogger<UsersController> logger, IRepository repository, IIdentityRepository identityRepository)
    : BaseEntityUserController<User, UserQueryCriteria>(logger, repository, identityRepository)
{
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
    /// Get new sign-ups.
    /// </summary>
    /// <param name="cancellationToken">The token used when request is cancelled.</param>
    /// <returns>The users.</returns>
    /// <response code="200">OK.</response>
    /// <response code="404">Not Found.</response>
    /// <response code="400">Bad Request.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="500">Error occurred.</response>
    [HttpGet]
    [Route("new-signups")]
    [AllowAnonymous]
    [ResponseCache(NoStore = true)]
    [Produces(HttpContentType.JSON)]
    [ProducesResponseType(typeof(IEnumerable<User>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public virtual async Task<IActionResult> GetNewSignUpsAsync(CancellationToken cancellationToken = default)
    {
        var user = await this.Repository
            .GetManyAsync<User>(x => x.CreatedAt > DateTimeOffset.UtcNow.AddHours(-2), cancellationToken);

        return this.Ok(user);
    }
}