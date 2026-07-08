using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Nano.Storage.Abstractions;

namespace Svc.Accounts.NewFolder.HealthChecks;

/// <summary>
/// Performs a health check against the mounted Azure File Share to verify its availability.
/// </summary>
/// <remarks>
///     The health check verifies that the storage share root is accessible by enumerating its contents, exercising the underlying
///     file share mount end-to-end. The share root is resolved from the injected <see cref="IPathProvider"/>.
/// </remarks>
public sealed class AzureFileshareStorageHealthCheck : IHealthCheck
{
    private readonly IPathProvider pathProvider;

    private static readonly TimeSpan probeTimeout = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Initializes a new instance of <see cref="AzureFileshareStorageHealthCheck"/>.
    /// </summary>
    /// <param name="pathProvider">A non-null <see cref="IPathProvider"/> providing the root path of the mounted storage share.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pathProvider"/> is <c>null</c>.</exception>
    public AzureFileshareStorageHealthCheck(IPathProvider pathProvider)
    {
        this.pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
    }

    /// <summary>
    /// Executes the health check asynchronously.
    /// </summary>
    /// <param name="context">The <see cref="HealthCheckContext"/> containing registration and failure status information.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the health check operation.</param>
    /// <returns>
    ///     A <see cref="HealthCheckResult"/> indicating whether the storage share root is accessible. Returns <see cref="HealthCheckResult.Healthy"/> when the root can be enumerated;
    ///     otherwise, returns a result with the configured failure status.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is <c>null</c>.</exception>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            var root = this.pathProvider.Root;

            var accessible = await IsPathAccessibleAsync(root, probeTimeout, cancellationToken);

            return accessible
                ? HealthCheckResult.Healthy("Storage mount accessible")
                : new HealthCheckResult(context.Registration.FailureStatus, $"Cannot access storage mount {root}");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private static async Task<bool> IsPathAccessibleAsync(string path, TimeSpan timeout, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(path);

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            var probeTask = Task.Run(() => Directory.EnumerateFileSystemEntries(path).FirstOrDefault(), timeoutCts.Token);

            await probeTask
                .WaitAsync(timeoutCts.Token);

            return true;
        }
        catch
        {
            return false;
        }
    }
}