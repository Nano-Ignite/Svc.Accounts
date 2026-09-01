using Nano.Data;
using Nano.Data.MySql;
using Nano.Data.SqlServer;

namespace Svc.Accounts.Data;

/// <inheritdoc />
public class AccountsDbContextFactory : BaseDbContextFactory<SqlServerProvider, AccountsDbContext>;