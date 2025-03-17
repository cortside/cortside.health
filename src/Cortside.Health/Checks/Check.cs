using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Cortside.Health.Checks {
    public abstract class Check {
        protected CheckConfiguration check;
        protected readonly IMemoryCache cache;
        protected readonly ILogger<Check> logger;
        protected readonly IAvailabilityRecorder recorder;
        protected readonly Availability availability = new Availability();

        protected Check(IMemoryCache cache, ILogger<Check> logger, IAvailabilityRecorder recorder) {
            this.cache = cache;
            this.logger = logger;
            this.recorder = recorder;
        }

        public void Initialize(CheckConfiguration cc) {
            check = cc;

            // fix up and make sure that interval and cache duration are set
            if (check.Interval == 0 && check.CacheDuration == 0) {
                check.Interval = 30;
            }
            if (check.Interval == 0 && check.CacheDuration > 0) {
                check.Interval = check.CacheDuration;
            }
            if (check.CacheDuration < check.Interval) {
                check.CacheDuration = check.Interval * 2;
            }
            if (check.Timeout == 0) {
                check.Timeout = check.Interval;
            }

            logger.LogInformation($"Initializing {check.Name} check of type {GetType().Name} with interval of {check.Interval}s, timeout of {check.Timeout}s and cache duration of {check.CacheDuration}s");
        }

        public string Name => check.Name;

        protected ServiceStatusModel Failure(string statusDetail) {
            return new ServiceStatusModel() {
                Healthy = false,
                Timestamp = DateTime.UtcNow,
                Status = ServiceStatus.Failure,
                StatusDetail = statusDetail,
                Required = check.Required
            };
        }

        public ServiceStatusModel Status {
            get {
                var status = cache.Get<ServiceStatusModel>(Name);
                status ??= Failure("status not cached");

                return status;
            }
        }

        public async Task<ServiceStatusModel> InternalExecuteAsync() {
            var item = cache.Get<ServiceStatusModel>(Name);
            var age = item != null ? (DateTime.UtcNow - item.Timestamp).TotalSeconds : int.MaxValue;
            if (age >= check.Interval) {
                logger.LogInformation($"Checking status of {Name}");

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var timespan = TimeSpan.FromSeconds(check.Timeout);
                ServiceStatusModel serviceStatusModel;
                try {
                    var task = ExecuteAsync();
                    using (var cts = new CancellationTokenSource(timespan)) {
                        serviceStatusModel = await task.WaitAsync(cts.Token);
                    }
                } catch (TaskCanceledException ex) {
                    serviceStatusModel = Failure("The operation timed out");
                } catch (Exception ex) {
                    serviceStatusModel = Failure(ex.Message);
                }

                stopwatch.Stop();


                if (!serviceStatusModel.Healthy) {
                    logger.LogError($"Check response for {Name} is failure with ServiceStatus of {serviceStatusModel.StatusDetail}: {JsonConvert.SerializeObject(serviceStatusModel)}");
                }

                var timeout = (int)timespan.TotalMilliseconds;
                if (stopwatch.ElapsedMilliseconds > timeout) {
                    logger.LogWarning($"Check of {Name} took {stopwatch.ElapsedMilliseconds}ms which is greater than configured Timeout of {timeout}ms");
                }

                availability.UpdateStatistics(serviceStatusModel.Healthy, stopwatch.ElapsedMilliseconds);
                serviceStatusModel.Availability = availability;
                recorder.RecordAvailability(Name, stopwatch.Elapsed, serviceStatusModel.Healthy, JsonConvert.SerializeObject(serviceStatusModel));

                // Store it in cache
                cache.Set(Name, serviceStatusModel, DateTimeOffset.Now.AddSeconds(check.CacheDuration));

                return serviceStatusModel;
            }

            return item;
        }

        // TODO: consider using a cancellation token so that the task can be cancelled if it takes too long
        public abstract Task<ServiceStatusModel> ExecuteAsync();
    }
}
