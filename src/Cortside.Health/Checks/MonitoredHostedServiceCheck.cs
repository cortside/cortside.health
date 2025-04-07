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


                    if (!int.TryParse(check.Value ?? "", out int age)) {
                        age = 120;
                    }
                    var services = monitoredServices.Where(x => x.LastActivity < DateTime.UtcNow.AddSeconds(-1 * age)).ToList();
                    if (services.Count > 0) {
                        serviceStatusModel.Healthy = false;
                        serviceStatusModel.Status = ServiceStatus.Failure;
                        var types = string.Join(',', services.Select(x => x.GetType().ToString()).ToArray());
                        serviceStatusModel.StatusDetail = $"Unhealthy services: {types}";
                    } else {
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

