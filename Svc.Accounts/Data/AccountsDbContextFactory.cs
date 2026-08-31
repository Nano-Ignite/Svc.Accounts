using Nano.Data;
using Nano.Data.MySql;
using Nano.Data.PostgreSQL;

namespace Svc.Accounts.Data;

/// <inheritdoc />
public class AccountsDbContextFactory : BaseDbContextFactory<PostgresSqlProvider, AccountsDbContext>;