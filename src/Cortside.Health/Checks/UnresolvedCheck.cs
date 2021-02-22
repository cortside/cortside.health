using System;
using System.Threading.Tasks;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Cortside.Health.Checks {

    public class UnresolvedCheck : Check {

        private readonly string statusDetail;

        public UnresolvedCheck(IMemoryCache cache, ILogger<Check> logger, IAvailabilityRecorder recorder, string statusDetail) : base(cache, logger, recorder) {
            this.statusDetail = $"Unable to resolve type for check with error: {statusDetail}";
        }

        public override async Task<ServiceStatusModel> ExecuteAsync() {
            var model = new ServiceStatusModel() {
                Healthy = false,
                Status = ServiceStatus.Failure,
                StatusDetail = statusDetail,
                Timestamp = DateTime.UtcNow
            };
            logger.LogError(statusDetail);
            return await Task.FromResult<ServiceStatusModel>(model);
        }
    }
}
