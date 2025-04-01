using System.Threading.Tasks;
using Cortside.Common.Hosting;
using Microsoft.Extensions.Logging;

namespace Cortside.Health.Tests.Hosting {
    public class MonitoredHostedService : TimedHostedService, IMonitoredHostedService {
        private readonly MonitoredHostedConfiguration config;

        public MonitoredHostedService(ILogger logger, MonitoredHostedConfiguration config) : base(logger, config.Enabled, config.Interval) {
            this.config = config;
        }

        protected override Task ExecuteIntervalAsync() {
            return Task.Delay(config.Sleep);
        }
    }
}
