using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineStore.Web.API.HealthChecks
{
    public class RandomHealthCheck : IHealthCheck
    {
        private const string Description = "Reports unhealthy status if random number > 15 and degraded if > 10";

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = new Random().Next(0, 30);

            if (result < 10)
            {
                return Task.FromResult(HealthCheckResult.Healthy(Description));
            }
            else if (result < 15)
            {
                return Task.FromResult(HealthCheckResult.Degraded(Description));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy(Description));
        }
    }
}
