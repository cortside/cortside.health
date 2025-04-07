using System;

namespace Cortside.Health.Tests.Hosting {
    public class MonitoredHostedConfiguration {
        public int Interval { get; set; }
        public bool Enabled { get; set; }
        public TimeSpan Sleep { get; set; }
    }
}
