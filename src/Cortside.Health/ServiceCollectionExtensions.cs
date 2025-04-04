using System;
using Cortside.Common.Validation;
using Cortside.Health.Checks;
using Cortside.Health.Recorders;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cortside.Health {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddHealth(this IServiceCollection services, Action<HealthOptions> options) {
            var o = new HealthOptions();
            options?.Invoke(o);

            return services.AddHealth(o);
        }

        public static IServiceCollection AddHealth(this IServiceCollection services, HealthOptions configuration, Action<CheckFactory> action = null) {
            Guard.From.Null(configuration, nameof(configuration));
            Guard.From.Null(configuration.ServiceConfiguration, nameof(configuration.ServiceConfiguration));
            Guard.From.Null(configuration.BuildModel, nameof(configuration.BuildModel));

            if (!string.IsNullOrWhiteSpace(configuration.TelemetryConfiguration?.ConnectionString)) {
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

                // allow for additional checks to be registered
                action?.Invoke(factory);

                return factory as ICheckFactory;
            });
            services.AddHostedService<HealthCheckHostedService>();

            return services;
        }
    }
}
