using Xunit;
using Microsoft.Extensions.Logging;
using WebApi.Controllers;

namespace WebApi.Tests
{
    public class WeatherForecastControllerTests
    {
        private ILogger<WeatherForecastController> _logger;
        
        [Fact]
        public void GetMethodTest()
        {
            // Assemble
            WeatherForecastController testController = new WeatherForecastController(_logger);
            var expected = typeof(WebApi.WeatherForecast[]);

            // Act
            var actual = testController.Get();

            // Assert
            Assert.IsType(expected, actual);
        }

        [Fact]
        public void GetMethodWithParamTest()
        {
            // Assemble
            WeatherForecastController testController = new WeatherForecastController(_logger);

            // Act
            var actual = testController.Get(5);

            // Assert
            Assert.NotNull(actual);
        }
    }
}