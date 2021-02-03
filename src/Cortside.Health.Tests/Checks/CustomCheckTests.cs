using System;
using System.Threading.Tasks;
using Cortside.Health.Checks;
using Cortside.Health.Contracts;
using Cortside.Health.Enums;
using Cortside.Health.Models;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Cortside.Health.Tests.Checks {
    public class CustomCheckTests {
        private readonly Mock<IMemoryCache> cacheMock;
        private readonly Mock<ILogger<Check>> loggerMock;
        private readonly Mock<IAvailabilityRecorder> recorderMock;
        private readonly CustomCheck customCheck;
        private readonly Mock<IHealthValidator> healthValidatorCheckMock;

        public CustomCheckTests() {
            this.healthValidatorCheckMock = new Mock<IHealthValidator>();
            this.cacheMock = new Mock<IMemoryCache>();
            this.loggerMock = new Mock<ILogger<Check>>();
            this.recorderMock = new Mock<IAvailabilityRecorder>();
            customCheck = new CustomCheck(cacheMock.Object, loggerMock.Object, recorderMock.Object, healthValidatorCheckMock.Object);
        }

        [Fact]
        public async Task ShouldExecuteHealthValidationSuccessfully() {
            // arrange
            var expectedModel = new ServiceStatusModel {
                Healthy = true,
                Status = ServiceStatus.Ok,
                StatusDetail = "Successful",
                Timestamp = DateTime.UtcNow,
                Required = true
            };

            healthValidatorCheckMock.Setup(validator => validator.ValidateStatus())
                .ReturnsAsync(new HealthValidationModel {
                    IsSuccessful = true,
                    ErrorMessage = null
                });

            // act
            customCheck.Initialize(new CheckConfiguration {
                Required = expectedModel.Required
            });
            var serviceStatusModel = await customCheck.ExecuteAsync();

            // assert
            serviceStatusModel.Should().NotBeNull();
            serviceStatusModel.Should().BeEquivalentTo(expectedModel, options => options.Excluding(opt => opt.Timestamp));
        }

        [Fact]
        public async Task ShouldExecuteHealthValidationUnsuccessfully() {
            // arrange
            var expectedModel = new ServiceStatusModel {
                Healthy = false,
                Status = ServiceStatus.Failure,
                StatusDetail = "Unknown error",
                Required = true
            };

            healthValidatorCheckMock.Setup(validator => validator.ValidateStatus())
                .ReturnsAsync(new HealthValidationModel {
                    IsSuccessful = false,
                    ErrorMessage = expectedModel.StatusDetail
                });

            // act
            customCheck.Initialize(new CheckConfiguration {
                Required = expectedModel.Required
            });
            var serviceStatusModel = await customCheck.ExecuteAsync();

            // assert
            serviceStatusModel.Should().NotBeNull();
            serviceStatusModel.Should().BeEquivalentTo(expectedModel, options => options.Excluding(opt => opt.Timestamp));
        }
    }
}
