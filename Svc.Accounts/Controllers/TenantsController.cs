using Microsoft.Extensions.Logging;
using Nano.App.Api.Controllers;
using Nano.Data.Abstractions;
using Nano.Eventing.Abstractions;
using Svc.Accounts.Models.Criterias;
using Svc.Accounts.Models.Data;

namespace Svc.Accounts.Controllers;

/// <inheritdoc />
public class TenantsController(ILogger<TenantsController> logger, IRepository repository, IEventing eventing)
    : BaseEntityController<Tenant, TenantQueryCriteria>(logger, repository, eventing);