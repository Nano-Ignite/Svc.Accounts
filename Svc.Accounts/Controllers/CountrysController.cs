using Microsoft.Extensions.Logging;
using Nano.App.Api.Controllers;
using Nano.Data.Abstractions;
using Nano.Eventing.Abstractions;
using Svc.Accounts.Models.Criterias;
using Svc.Accounts.Models.Data;

namespace Svc.Accounts.Controllers;

/// <inheritdoc />
public class CountrysController(ILogger<CountrysController> logger, IRepository repository, IEventing eventing)
    : BaseEntityController<Country, CountryQueryCriteria>(logger, repository, eventing);