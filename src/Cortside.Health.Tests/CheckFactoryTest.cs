using Cortside.Health.Checks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cortside.Health.Tests {
    public class CheckFactoryTest {
        public readonly IServiceCollection serviceCollection;
        public CheckFactoryTest() {
            this.serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<UrlCheck>();
            serviceCollection.AddTransient<DbContextCheck>();
        }

        [Fact]
        public void ShouldResolveUrlCheck() {
            // assert
            var cache = new Mock<IMemoryCache>();
            var logger = new Mock<ILogger<Check>>();
            var recorder = new Mock<IAvailabilityRecorder>();
            var configuration = new Mock<IConfiguration>();
            var sp = serviceCollection.BuildServiceProvider();
            var factory = new CheckFactory(cache.Object, logger.Object, recorder.Object, sp, configuration.Object);

            // act
            var config = new CheckConfiguration() { Name = "foo", Type = "url" };
            var check = factory.Create(config);

            // assert
            check.Should().NotBeNull();
        }

        [Fact]
        public void ShouldResolveUnresolvedCheckForNullTypeName() {
            // assert
            var cache = new Mock<IMemoryCache>();
            var logger = new Mock<ILogger<Check>>();
            var recorder = new Mock<IAvailabilityRecorder>();
            var configuration = new Mock<IConfiguration>();
            var sp = serviceCollection.BuildServiceProvider();
            var factory = new CheckFactory(cache.Object, logger.Object, recorder.Object, sp, configuration.Object);

            // act
            var config = new CheckConfiguration() { Name = "foo", Type = null };
            var check = factory.Create(config);

            // assert
            check.Should().NotBeNull();
            check.Should().BeOfType(typeof(UnresolvedCheck));
        }

        [Fact]
        public void ShouldResolveUnresolvedCheckForMissingTypeName() {
            // assert
            var cache = new Mock<IMemoryCache>();
            var logger = new Mock<ILogger<Check>>();
            var recorder = new Mock<IAvailabilityRecorder>();
            var configuration = new Mock<IConfiguration>();
            var sp = serviceCollection.BuildServiceProvider();
            var factory = new CheckFactory(cache.Object, logger.Object, recorder.Object, sp, configuration.Object);

            // act
            var config = new CheckConfiguration() { Name = "foo", Type = "xxx" };
            var check = factory.Create(config);

            // assert
            check.Should().NotBeNull();
            check.Should().BeOfType(typeof(UnresolvedCheck));
        }

        [Fact]
        public void ShouldResolveUnresolvedCheckForUnregisteredType() {
            // assert
            var cache = new Mock<IMemoryCache>();
            var logger = new Mock<ILogger<Check>>();
            var recorder = new Mock<IAvailabilityRecorder>();
            var configuration = new Mock<IConfiguration>();
            var sp = serviceCollection.BuildServiceProvider();
            var factory = new CheckFactory(cache.Object, logger.Object, recorder.Object, sp, configuration.Object);
            factory.RegisterCheck("foo", typeof(HealthCheck));

            // act
            var config = new CheckConfiguration() { Name = "foo", Type = "foo" };
            var check = factory.Create(config);

            // assert
            check.Should().NotBeNull();
            check.Should().BeOfType(typeof(UnresolvedCheck));
        }
    }
}
