using System.Diagnostics;
using System.Threading.Tasks;
using Cortside.Health.Checks;
using Cortside.Health.Tests.Checks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using Xunit;

namespace Cortside.Health.Tests {
    public class CustomUrlCheckTest {
        [Fact]
        public async Task ShouldTimeoutAndFailCheckUrl() {
            // arrange
            var recorder = new Mock<IAvailabilityRecorder>();
            var check = new CustomUrlCheck(new MemoryCache(new MemoryCacheOptions()), new NullLogger<Check>(), recorder.Object);
            var cc = new CheckConfiguration {
                Name = "foo",
                Type = "url",
                Required = false,
                Value = "https://httpstat.us/200?sleep=10000",
                Interval = 30,
                Timeout = 5
            };
            check.Initialize(cc);

            // act
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var status = await check.InternalExecuteAsync();
            stopwatch.Stop();

            // assert
            stopwatch.ElapsedMilliseconds.ShouldBeLessThan(6000);
            Assert.False(status.Healthy);
            Assert.Equal("The operation timed out", status.StatusDetail);
        }
    }
}
