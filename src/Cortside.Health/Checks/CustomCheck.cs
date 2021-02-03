using System;
using System.Threading.Tasks;
using Cortside.Health.Contracts;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Cortside.Health.Checks {
    /// <summary>
    /// This check is intended to be used when a custom validation needs to be executed
    /// </summary>
    public class CustomCheck : Check {
        private readonly IHealthValidator healthValidator;

        public CustomCheck(IMemoryCache cache, ILogger<Check> logger, IAvailabilityRecorder recorder, IHealthValidator healthValidator) : base(cache, logger, recorder) {
            this.healthValidator = healthValidator;
        }

        public override async Task<ServiceStatusModel> ExecuteAsync() {
            var response = await healthValidator.ValidateStatus();

            return new ServiceStatusModel() {
                Healthy = response.IsSuccessful,
                Status = response.IsSuccessful ? ServiceStatus.Ok : ServiceStatus.Failure,
                StatusDetail = response.IsSuccessful ? "Successful" : response.ErrorMessage,
                Timestamp = DateTime.UtcNow,
                Required = check.Required
            };
        }
    }
}
