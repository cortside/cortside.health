using System;
using System.Threading.Tasks;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Cortside.Health.Checks {

    public class FailedCheck : Check {

        private string statusDetail;

        public FailedCheck(IMemoryCache cache, ILogger<Check> logger, IAvailabilityRecorder recorder, Type type) : base(cache, logger, recorder) {
            statusDetail = $"Unable to resolve type {type.Name} for check {check.Name}";
        }

        public override async Task<ServiceStatusModel> ExecuteAsync() {
            return new ServiceStatusModel() {
                Healthy = false,
                Status = ServiceStatus.Failure,
                StatusDetail = statusDetail,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
