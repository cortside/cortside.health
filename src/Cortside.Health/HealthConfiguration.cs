using System;
using System.Collections.Generic;
using Cortside.Health.Models;
using Microsoft.ApplicationInsights.Extensibility;

namespace Cortside.Health {
    public class HealthConfiguration {
        public TelemetryConfiguration TelemetryConfiguration { get; set; }
        public HealthCheckServiceConfiguration ServiceConfiguration { get; set; }
        public BuildModel BuildModel { get; set; }
        public Dictionary<string, Type> CustomChecks { get; set; }
    }
}
