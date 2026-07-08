using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nano.Storage.Abstractions;

namespace Svc.Accounts.NewFolder.HealthChecks.Extensions;

internal static class HealthChecksBuilderExtensions
{
    private const string NAME = "azure-fileshare";

    internal static IHealthChecksBuilder AddAzureFileshareStorage(this IHealthChecksBuilder builder, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder
            .Add(new HealthCheckRegistration(NAME, x =>
            {
                var pathProvider = x
                    .GetRequiredService<IPathProvider>();

                return new AzureFileshareStorageHealthCheck(pathProvider);
            }, failureStatus, tags, timeout));

        return builder;
    }
}