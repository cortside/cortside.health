using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cortside.Common.Hosting;
using Cortside.Health.Checks;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cortside.Health {

    /// <summary>
    /// Background service for health check
    /// </summary>
    public class HealthCheckHostedService : TimedHostedService {

        private readonly List<Check> checks;
        private readonly HealthCheckServiceConfiguration config;

        public HealthCheckHostedService(ILogger<HealthCheckHostedService> logger, HealthCheckServiceConfiguration config, IMemoryCache cache, IConfiguration configuration, ICheckFactory factory, BuildModel build) : base(logger, config.Enabled, config.Interval, true) {
            config.Name = factory.ExpandTemplate(config.Name);
            checks = new List<Check>();
            foreach (var check in config.Checks) {
                checks.Add(factory.Create(check));
            }

            var healthCheck = new HealthCheck(config, checks, build, cache, factory.Logger, factory.Recorder);
            healthCheck.Initialize(new CheckConfiguration() {
                Name = config.Name,
                Interval = config.Interval,
                CacheDuration = config.CacheDuration, Timeout = config.Interval
            });
            checks.Add(healthCheck);

            this.config = config;
        }

        public override Task StartAsync(CancellationToken cancellationToken) {
            logger.LogInformation("HealthCheckHostedService StartAsync() entered.");
            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteIntervalAsync() {
            int batches = (int)Math.Ceiling((double)checks.Count / config.BatchSize);
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            for (int i = 0; i < batches; i++) {
                var tasks = checks.Skip(i * config.BatchSize).Take(config.BatchSize).Select(t => t.InternalExecuteAsync());
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            stopwatch.Stop();

            var duration = (int)TimeSpan.FromSeconds(config.CacheDuration).TotalMilliseconds;
            if (stopwatch.ElapsedMilliseconds > duration) {
                logger.LogError($"Checks for service {config.Name} took {stopwatch.ElapsedMilliseconds}ms which is greater than configured CacheDuration of {duration}ms");
            }
        }
    }
}
