using System;
using System.Threading;
using System.Threading.Tasks;
using Cortside.Health.Checks;
using Cortside.Health.Tests.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Cortside.Health.Tests {
    public class MonitoredHostedServiceTest {
        [Theory]
        [InlineData(1, true, 1)]
        [InlineData(60000, false, 10)]
        public async Task ShouldGetHealth(int sleepMs, bool healthy, int waitDelay) {
            // arrange
            var logger = new NullLogger<MonitoredHostedService>();
            var config = new MonitoredHostedConfiguration {
                Enabled = true,
                Interval = 1,
                Sleep = TimeSpan.FromMilliseconds(sleepMs)
            };
            var service = new MonitoredHostedService(logger, config);

            // act
            var source = new CancellationTokenSource();
            var t1 = service.StartAsync(source.Token);
            var t2 = Task.Delay(10000);
            await Task.WhenAll(t1, t2);

            var services = new ServiceCollection();
            services.AddSingleton<IHostedService>(service);
            var serviceProvider = services.BuildServiceProvider();

            var recorder = new Mock<IAvailabilityRecorder>();
            var check = new MonitoredHostedServiceCheck(new MemoryCache(new MemoryCacheOptions()), new NullLogger<Check>(), serviceProvider, recorder.Object);
            var cc = new CheckConfiguration {
                Name = "foo",
                Type = "monitoredhostedservice",
                Required = true,
                Interval = 30,
                Timeout = 5
            };
            check.Initialize(cc);
            var status = await check.ExecuteAsync();

            // assert
            Assert.Equal(healthy, status.Healthy);
            await source.CancelAsync();
        }
    }
}
