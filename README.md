# Cortside.Health
Framework for exposing health check  endpoint with configurable checks and ability to publish available via telemetry recorder.

## Example appsettings.json configuration:
```json
"HealthCheckHostedService": {
    "Name": "{{Service:Name}}",
    "Enabled": true,
    "Interval": 5,
    "CacheDuration": 30,
    "Checks": [
        {
            "Name": "webapistarter-db",
            "Type": "dbcontext",
            "Required": true,
            "Interval": 30,
            "Timeout": 5
        },
        {
            "Name": "policyserver",
            "Type": "url",
            "Required": false,
            "Value": "{{PolicyServer:PolicyServerUrl}}/health",
            "Interval": 30,
            "Timeout": 5
        },
        {
            "Name": "identityserver",
            "Type": "url",
            "Required": false,
            "Value": "{{IdentityServer:Authority}}/api/health",
            "Interval": 30,
            "Timeout": 5
        }
    ]
},
"ApplicationInsights": {
    "InstrumentationKey": "",
    "EndpointAddress": "https://dc.services.visualstudio.com/v2/track"
}
```
Note: you can use the {{variable}} syntax in the name and value properties of the check to reference a configuration variable else where in configurations.  This way you are not duplicating values, example for services that have another section so that you don't have mismatch in check and running value.

## Example Startup.cs configuration:
```csharp
// use PartManager to register controller -- add to existing AddControllers call
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
// this one is not explicitly required, unless you want to use custom checks
services.AddTransient<CustomCheck>();

// check factory and hosted service
services.AddTransient<ICheckFactory, CheckFactory>();
services.AddHostedService<HealthCheckHostedService>();

// for DbContextCheck
services.AddTransient<DbContext, DatabaseContext>();

// for CustomCheck, we need to inject an explict implementation of IHealthValidator, to make sure that it is resolved as expected
services.AddTransient<IHealthValidator, <CustomClass>>();
```

## Running example
See Cortside.WebApiStarter as a working example of full api service that make use of health checks at https://github.com/cortside/cortside.webapistarter.