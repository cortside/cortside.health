using System;
using System.Collections.Generic;
using Cortside.Health.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

namespace Cortside.Health {
    public class HealthOptions {
        public HealthOptions() {
            CustomChecks = new Dictionary<string, Type>();
        }

        public HealthOptions(IConfiguration configuration) {
            TelemetryConfiguration = new TelemetryConfiguration() {
                ConnectionString = configuration["ApplicationInsights:ConnectionString"]
            };
            ServiceConfiguration = configuration.GetSection("HealthCheckHostedService").Get<HealthCheckServiceConfiguration>();
            BuildModel = configuration.GetSection("Build").Get<BuildModel>();
            CustomChecks = new Dictionary<string, Type>();
        }

        public TelemetryConfiguration TelemetryConfiguration { get; set; }
        public HealthCheckServiceConfiguration ServiceConfiguration { get; set; }
        public BuildModel BuildModel { get; set; }
        public Dictionary<string, Type> CustomChecks { get; set; }

        public void UseConfiguration(IConfiguration configuration) {
            var connectionString = configuration["ApplicationInsights:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(connectionString)) {
                TelemetryConfiguration = new TelemetryConfiguration() {
                    ConnectionString = configuration["ApplicationInsights:ConnectionString"]
                };
            }
            ServiceConfiguration = configuration.GetSection("HealthCheckHostedService").Get<HealthCheckServiceConfiguration>();
            BuildModel = configuration.GetSection("Build").Get<BuildModel>();
        }

        public void AddCustomCheck(string key, Type value) {
            CustomChecks.Add(key, value);
        }
    }
}
