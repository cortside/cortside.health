using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cortside.Health.Checks;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Cortside.Health.Tests.Checks {
    public class CustomUrlCheck : Check {
        public CustomUrlCheck(IMemoryCache cache, ILogger<Check> logger, IAvailabilityRecorder recorder) : base(cache,
            logger, recorder) {
        }

        public override async Task<ServiceStatusModel> ExecuteAsync() {
            var status = new ServiceStatusModel() {
                Healthy = false,
                Status = ServiceStatus.Failure,
                StatusDetail = "Failed to process response",
                Timestamp = DateTime.UtcNow,
                Required = check.Required
            };

            // leaving the default timeout of 100 seconds
            using (var httpClient = new HttpClient()) {
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
