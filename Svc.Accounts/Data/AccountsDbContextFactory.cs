using Nano.Data;
using Nano.Data.MySql;

namespace Svc.Accounts.Data;

/// <inheritdoc />
public class AccountsDbContextFactory : BaseDbContextFactory<PostgresSqlProvider2, AccountsDbContext>;