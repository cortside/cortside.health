using System;
using System.Linq;
using System.Threading.Tasks;
using Cortside.Common.Hosting;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cortside.Health.Checks {
    public class MonitoredHostedServiceCheck : Check {
        private readonly IServiceProvider serviceProvider;

        public MonitoredHostedServiceCheck(IMemoryCache cache, ILogger<Check> logger, IServiceProvider serviceProvider, IAvailabilityRecorder recorder) : base(cache, logger, recorder) {
            this.serviceProvider = serviceProvider;
        }

        public override async Task<ServiceStatusModel> ExecuteAsync() {
            var serviceStatusModel = new ServiceStatusModel() {
                Healthy = false,
                Required = check.Required,
                Timestamp = DateTime.UtcNow
            };

            using (var scope = serviceProvider.CreateScope()) {
                try {
                    var monitoredServices = serviceProvider.GetServices<IHostedService>()
                        .Where(x =>
                            x.GetType().GetInterfaces().Any(y => y == typeof(IMonitoredHostedService))
                        ).Select(x => x as IMonitoredHostedService)
                        .ToList();

                    // TODO: -5 should be configurable
                    var unhealthy = monitoredServices.Any(x => x.LastActivity < DateTime.UtcNow.AddSeconds(-5 * x.Interval.TotalSeconds));
                    if (!unhealthy) {
                        serviceStatusModel.Healthy = true;
                        serviceStatusModel.Status = ServiceStatus.Ok;
                        serviceStatusModel.StatusDetail = "Successful";
                    }
                } catch (Exception ex) {
                    serviceStatusModel.Status = ServiceStatus.Failure;
                    serviceStatusModel.StatusDetail = ex.Message;
                }
            }

            return serviceStatusModel;
        }
    }
}

