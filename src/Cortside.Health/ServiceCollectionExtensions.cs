using Cortside.Health.Checks;
using Cortside.Health.Recorders;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cortside.Health {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddHealth(this IServiceCollection services, HealthConfiguration configuration) {
            if (!string.IsNullOrEmpty(configuration.TelemetryConfiguration?.ConnectionString)) {
                TelemetryClient telemetryClient = new TelemetryClient(configuration.TelemetryConfiguration);
                services.AddSingleton(telemetryClient);
                services.AddTransient<IAvailabilityRecorder, ApplicationInsightsRecorder>();
            } else {
                services.AddTransient<IAvailabilityRecorder, NullRecorder>();
            }

            // configuration
            services.AddSingleton(configuration.ServiceConfiguration);
            services.AddSingleton(configuration.BuildModel);

            // checks
            services.AddTransient<UrlCheck>();
            services.AddTransient<DbContextCheck>();
            foreach (var check in configuration.CustomChecks) {
                services.AddTransient(check.Value);
            }

            // check factory and hosted service
            services.AddSingleton(sp => {
                var cache = sp.GetService<IMemoryCache>();
                var logger = sp.GetService<ILogger<Check>>();
                var recorder = sp.GetService<IAvailabilityRecorder>();
                var settings = sp.GetService<IConfiguration>();

                var factory = new CheckFactory(cache, logger, recorder, sp, settings);
                foreach (var check in configuration.CustomChecks) {
                    factory.RegisterCheck(check.Key, check.Value);
                }
                return factory as ICheckFactory;
            });
            services.AddHostedService<HealthCheckHostedService>();

            return services;
        }
    }
}
