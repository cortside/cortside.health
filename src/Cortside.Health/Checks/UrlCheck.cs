using System;
using System.Net.Http;
using System.Threading.Tasks;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Cortside.Health.Checks {
    public class UrlCheck : Check {
        public UrlCheck(IMemoryCache cache, ILogger<Check> logger, IAvailabilityRecorder recorder) : base(cache, logger, recorder) { }

        public override async Task<ServiceStatusModel> ExecuteAsync() {
            var status = new ServiceStatusModel() {
                Healthy = false,
                Status = ServiceStatus.Failure,
                StatusDetail = "Failed to process response",
                Timestamp = DateTime.UtcNow,
                Required = check.Required
            };

            var httpClient = new HttpClient {
                Timeout = TimeSpan.FromSeconds(check.Timeout)
            };

            using (var response = await httpClient.GetAsync(check.Value, HttpCompletionOption.ResponseHeadersRead)) {
                if (response.IsSuccessStatusCode) {
                    status.Healthy = true;
                    status.Status = ServiceStatus.Ok;
                    status.StatusDetail = "Successful";
                } else {
                    status.StatusDetail = await response.Content.ReadAsStringAsync();
                }
            }

            return status;
        }
    }
}
