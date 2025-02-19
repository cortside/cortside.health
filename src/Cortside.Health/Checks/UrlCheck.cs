using System;
using System.IO;
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

            using (var httpClient = new HttpClient {
                Timeout = TimeSpan.FromSeconds(check.Timeout)
            }) {
                try {
                    var response = await httpClient.GetAsync(check.Value, HttpCompletionOption.ResponseHeadersRead);
                    if (response.IsSuccessStatusCode) {
                        status.Healthy = true;
                        status.Status = ServiceStatus.Ok;
                        status.StatusDetail = "Successful";
                    } else {
                        status.StatusDetail = await response.Content.ReadAsStringAsync();
                    }

                } catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException) {
                    // Handle timeout -- .net 5+
                    status.StatusDetail = "The operation timed out";
                } catch (TaskCanceledException ex) when (ex.InnerException is IOException) {
                    // Handle timeout -- < .net 5
                    status.StatusDetail = "The operation timed out";
                } catch (TaskCanceledException ex) {
                    // Handle cancellation.
                    status.StatusDetail = ex.Message;
                }
            }
            return status;
        }
    }
}
