using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cortside.Health.Checks {
    public class HealthCheck : Check {
        private readonly BuildModel build;
        private readonly List<Check> checks;
        private readonly HealthCheckServiceConfiguration config;

        public HealthCheck(HealthCheckServiceConfiguration config, List<Check> checks, BuildModel build, IMemoryCache cache, ILogger<Check> logger, IAvailabilityRecorder recorder) : base(cache, logger, recorder) {
            this.build = build;
            this.checks = checks;
            this.config = config;
        }

        public override async Task<ServiceStatusModel> ExecuteAsync() {
            var response = await Task.Run(() => new HealthModel() {
                Service = Name,
                Build = build,
                Checks = new Dictionary<string, ServiceStatusModel>(),
                Timestamp = DateTime.UtcNow,
                Uptime = GetRunningTime()
            });

            foreach (var check in checks.Where(c => c.Name != Name)) {
                response.Checks.Add(check.Name, check.Status);
            }

            response.Healthy = !response.Checks.Select(x => x.Value).Any(c => c.Required && !c.Healthy);
            response.Status = response.Healthy ? ServiceStatus.Ok : ServiceStatus.Failure;
            var degraded = response.Checks.Select(x => x.Value).Any(c => !c.Required && !c.Healthy);

            if (response.Healthy && degraded) {
                response.Status = ServiceStatus.Degraded;
            }

            if (response.Uptime.TotalSeconds > config.CacheDuration) {
                if (response.Status == ServiceStatus.Failure) {
                    logger.LogError($"Health check response for {Name} is failure: {JsonConvert.SerializeObject(response.Checks.Where(c => !c.Value.Healthy).ToDictionary(c => c.Key, c => c.Value))}");
                } else if (response.Status == ServiceStatus.Degraded) {
                    logger.LogWarning($"Health check response for {Name} is degraded: {JsonConvert.SerializeObject(response.Checks.Where(c => !c.Value.Healthy).ToDictionary(c => c.Key, c => c.Value))}");
                }
            }

            return response;
        }

        private static TimeSpan GetRunningTime() {
            var runningTime = TimeSpan.MaxValue;
            try {
                Process currentProcess = Process.GetCurrentProcess();
                if (currentProcess != null) {
                    runningTime = DateTime.UtcNow.Subtract(currentProcess.StartTime);
                }
            } catch {
                // ignore this, not critical
            }

            return runningTime;
        }
    }
}
