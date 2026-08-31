using Azure.Core;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nano.Common.Mvc.HealthChecks.Extensions;
using Nano.Data.Abstractions;
using Nano.Data.Abstractions.Config;
using Nano.Data.Abstractions.Exceptions;
using Nano.Data.Extensions;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System;

namespace Nano.Data.MySql;

/// <summary>
/// PostgreSQL data provider using Npgsql.
/// </summary>
/// <remarks>
///     Supports retry policies, batching, spatial data via NetTopologySuite, query splitting behavior, and optional health checks.
///     Documentation: https://github.com/Nano-Core/Nano.Library/blob/master/Nano.Data.PostgreSQL/README.md#nanodatapostgresql.
/// </remarks>
public sealed class PostgresSqlProvider2 : IDataProvider
{
    /// <inheritdoc />
    public static void Configure(IServiceCollection services, DataOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services
            .AddSingleton<IDatabaseExceptionTranslator, PostgreSqlExceptionTranslator2>();

        if (options.HealthCheck != null)
        {
            var failureStatus = options.HealthCheck.UnhealthyStatus
                .GetHealthStatus();

            services
                .AddHealthChecks()
                .AddNpgSql(options.ConnectionString, name: "postgres", failureStatus: failureStatus);
        }
    }

    /// <inheritdoc />
    public static void Configure(DbContextOptionsBuilder builder, DataOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        var batchSize = options.BatchSize;
        var retryCount = options.QueryRetryCount;
        var connectionString = options.ConnectionString;

        void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder x)
        {
            var querySplittingBehavior = options.QuerySplittingBehavior
                .GetQuerySplittingBehavior();

            x.MaxBatchSize(batchSize);
            x.EnableRetryOnFailure(retryCount);
            x.UseNetTopologySuite();
            x.UseQuerySplittingBehavior(querySplittingBehavior);
        }

        if (true)
        {
            const string DEFAULT_URL = "https://ossrdbms-aad.database.windows.net/.default";

            var credential = new WorkloadIdentityCredential();
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

            dataSourceBuilder
                .UsePeriodicPasswordProvider(
                    async (_, cancellationToken) =>
                    {
                        var request = new TokenRequestContext([DEFAULT_URL]);

                        var token = await credential
                            .GetTokenAsync(request, cancellationToken);

                        return token.Token;
                    }, TimeSpan.FromMinutes(50), TimeSpan.FromSeconds(10));

            builder
                .UseNpgsql(dataSourceBuilder.Build(), ConfigureNpgsql);
        }
        //else
        //{
        //    builder
        //        .UseNpgsql(connectionString, ConfigureNpgsql);
        //}
    }
}


internal sealed class PostgreSqlExceptionTranslator2 : IDatabaseExceptionTranslator
{
    public Exception Translate(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is DbUpdateException { InnerException: PostgresException { SqlState: "23505" } } dbUpdateException)
        {
            return new UniqueConstraintViolationException(dbUpdateException);
        }

        return exception;
    }
}