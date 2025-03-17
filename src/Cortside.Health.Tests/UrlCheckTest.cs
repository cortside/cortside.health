using System.Threading.Tasks;
using Cortside.Health.Checks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Cortside.Health.Tests {
    public class UrlCheckTest {
        [Fact]
        public async Task ShouldPassCheckUrl() {
            // arrange
            var recorder = new Mock<IAvailabilityRecorder>();
            var check = new UrlCheck(new MemoryCache(new MemoryCacheOptions()), new NullLogger<Check>(), recorder.Object);
            var cc = new CheckConfiguration {
                Name = "foo",
                Type = "url",
                Required = false,
                Value = "https://httpstat.us/200",
                Interval = 30,
                Timeout = 5
            };
            check.Initialize(cc);

            // act
            var status = await check.ExecuteAsync();

            // assert
            Assert.True(status.Healthy);
        }

        [Fact]
        public async Task ShouldFailCheckUrl() {
            // arrange
            var recorder = new Mock<IAvailabilityRecorder>();
            var check = new UrlCheck(new MemoryCache(new MemoryCacheOptions()), new NullLogger<Check>(), recorder.Object);
            var cc = new CheckConfiguration {
                Name = "foo",
                Type = "url",
                Required = false,
                Value = "https://httpstat.us/503",
                Interval = 30,
                Timeout = 5
            };
            check.Initialize(cc);

            // act
            var status = await check.ExecuteAsync();

            // assert
            Assert.False(status.Healthy);
            Assert.Equal("503 Service Unavailable", status.StatusDetail);
        }

        [Fact]
        public async Task ShouldTimeoutAndFailCheckUrl() {
            // arrange
            var recorder = new Mock<IAvailabilityRecorder>();
            var check = new UrlCheck(new MemoryCache(new MemoryCacheOptions()), new NullLogger<Check>(), recorder.Object);
            var cc = new CheckConfiguration {
                Name = "foo",
                Type = "url",
                Required = false,
                Value = "https://httpstat.us/200?sleep=15000",
                Interval = 30,
                Timeout = 5
            };
            check.Initialize(cc);

            // act
            var status = await check.ExecuteAsync();

            // assert
            Assert.False(status.Healthy);
            Assert.Equal("The operation timed out", status.StatusDetail);
        }
    }
}
