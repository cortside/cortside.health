using Cortside.Health.Checks;
using Cortside.Health.Contracts;
using Cortside.Health.Models;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cortside.Health.Tests {
    public class CheckFactoryTest {
        private readonly IServiceCollection serviceCollection;
        private readonly Mock<IMemoryCache> cacheMock;
        private readonly Mock<ILogger<Check>> loggerMock;
        private readonly Mock<IAvailabilityRecorder> recorderMock;
        private readonly Mock<IConfiguration> configurationMock;
        private readonly Mock<IHealthValidator> healthValidatorCheckMock;

        public CheckFactoryTest() {
            this.healthValidatorCheckMock = new Mock<IHealthValidator>();
            this.serviceCollection = new ServiceCollection();
            this.cacheMock = new Mock<IMemoryCache>();
            this.loggerMock = new Mock<ILogger<Check>>();
            this.recorderMock = new Mock<IAvailabilityRecorder>();
            this.configurationMock = new Mock<IConfiguration>();
            serviceCollection.AddTransient<UrlCheck>();
            serviceCollection.AddTransient<DbContextCheck>();
            serviceCollection.AddTransient<CustomCheck>();
            serviceCollection.AddSingleton(this.cacheMock.Object);
            serviceCollection.AddSingleton(this.loggerMock.Object);
            serviceCollection.AddSingleton(this.recorderMock.Object);
            serviceCollection.AddSingleton(this.configurationMock.Object);
            serviceCollection.AddSingleton(this.healthValidatorCheckMock.Object);
        }

        [Fact]
        public void ShouldResolveCustomCheck() {
            // assert
            var sp = serviceCollection.BuildServiceProvider();
            var factory = new CheckFactory(cacheMock.Object, loggerMock.Object, recorderMock.Object, sp, configurationMock.Object);

            // act
            var config = new CheckConfiguration() { Name = "custom-check", Type = "custom", Required = true, Timeout = 5 };
            var check = factory.Create(config);

            // assert
            check.Should().NotBeNull();
            check.Should().BeOfType<CustomCheck>();
        }

        [Fact]
        public void ShouldResolveUrlCheck() {
            // assert
            var sp = serviceCollection.BuildServiceProvider();
            var factory = new CheckFactory(cacheMock.Object, loggerMock.Object, recorderMock.Object, sp, configurationMock.Object);

            // act
            var config = new CheckConfiguration() { Name = "foo", Type = "url" };
            var check = factory.Create(config);

            // assert
            check.Should().NotBeNull();
            check.Should().BeOfType<UrlCheck>();
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
