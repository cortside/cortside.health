using System.Collections.Generic;
using System.Dynamic;

namespace Cortside.Health {
    /// <summary>
    /// config
    /// </summary>
    public class HealthCheckServiceConfiguration {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public int Interval { get; set; }
        public int CacheDuration { get; set; }
        public int BatchSize { get; set; } = 5;
        public List<CheckConfiguration> Checks { get; set; }
    }

    public class CheckConfiguration : DynamicObject {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public bool Required { get; set; }
        public int CacheDuration { get; set; }
        public int Timeout { get; set; }
        public int Interval { get; set; }
    }
}
