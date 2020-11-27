using System;
using System.Threading.Tasks;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cortside.Health.Checks {
    public class DbContextCheck : Check {
        private readonly IServiceProvider serviceProvider;

        public DbContextCheck(IMemoryCache cache, ILogger<Check> logger, IServiceProvider serviceProvider, IAvailabilityRecorder recorder) : base(cache, logger, recorder) {
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
                    var context = scope.ServiceProvider.GetRequiredService<DbContext>();

                    // only do actual test to conext if the db is not the in memory provider
                    if (context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory") {
                        context.Database.SetCommandTimeout(check.Timeout);
                        await context.Database.ExecuteSqlCommandAsync("select @@VERSION");
                    }

                    serviceStatusModel.Healthy = true;
                    serviceStatusModel.Status = ServiceStatus.Ok;
                    serviceStatusModel.StatusDetail = "Successful";
                } catch (Exception ex) {
                    serviceStatusModel.Status = ServiceStatus.Failure;
                    serviceStatusModel.StatusDetail = ex.Message;
                }
            }

            return serviceStatusModel;
        }
    }
}

