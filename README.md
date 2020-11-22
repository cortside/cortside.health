# Cortside.Health
Framework for exposing health check  endpoint with configurable checks and ability to publish available via telemetry recorder.

## Example Startup.cs configuration:
```csharp
// use PartManager to register controller
services.AddControllers()
    .PartManager.ApplicationParts.Add(new AssemblyPart(typeof(HealthController).Assembly));

// health checks
// telemetry recorder
string instrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
string endpointAddress = Configuration["ApplicationInsights:EndpointAddress"];
if (!string.IsNullOrEmpty(instrumentationKey) && !string.IsNullOrEmpty(endpointAddress)) {
    TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration(instrumentationKey, new InMemoryChannel { EndpointAddress = endpointAddress });
    TelemetryClient telemetryClient = new TelemetryClient(telemetryConfiguration);
    services.AddSingleton(telemetryClient);
    services.AddTransient<IAvailabilityRecorder, ApplicationInsightsRecorder>();
} else {
    services.AddTransient<IAvailabilityRecorder, NullRecorder>();
}

// configuration
services.AddSingleton(Configuration.GetSection("HealthCheckHostedService").Get<HealthCheckServiceConfiguration>());
services.AddSingleton(Configuration.GetSection("Build").Get<BuildModel>());

// checks
services.AddTransient<UrlCheck>();
services.AddTransient<DbContextCheck>();

// check factory and hosted service
services.AddTransient<ICheckFactory, CheckFactory>();
services.AddHostedService<HealthCheckHostedService>();

// for DbContextCheck
services.AddTransient<DbContext, DatabaseContext>();
```

## Running example
See Cortside.WebApiStarter as a working example of full api service that make use of health checks at https://github.com/cortside/cortside.webapistarter.