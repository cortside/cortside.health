using System.Collections.Generic;
using Cortside.Health.Checks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Cortside.Health.Tests {
    public class ServiceCollectionTest {
        [Fact]
        public void ShouldAddHealthWithoutApplicationInsights() {
            var myConfiguration = new Dictionary<string, string>() {
                {"HealthCheckHostedService:Enabled", "true"},
                {"Build:version", "1.0.0"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var services = new ServiceCollection();

            services.AddHealth(o => {
                o.UseConfiguration(configuration);
                o.AddCustomCheck("foo", typeof(UrlCheck));
            });
        }
    }
}
