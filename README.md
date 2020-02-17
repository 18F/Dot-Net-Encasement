# .NET Core / Web API Tutorial

This part of the tutorial will show how to set up some simple tests for WebAPI Controllers. We'll revisit test writing again [in the next part of the tutorial](../../tree/part-3), which will cover development of a controller to access an existing REST service.

## Part 2: Creating Web API Controller Tests

In the integrated terminal in Visual Studio Code, navigate to the `WebApi.Tests` directory. Create a new unit test project:

```bash
$ dotnet new xunit
```

Change the name of the generated test file to `WeatherForecastControllerTests.cs` to be explicit about the controller that these tests will cover. Add a reference to the project that you are writing controller tests for:

```bash
$ dotnet add reference ../WebApi/WebApi.csproj
```

In the test file, change the name of the newly created class - a useful convention to use is to name your class based on the controller you are writing tests for. In this case, we'll use `WeatherForecastControllerTests`. You'll also need to add a `using` statement to reference the assembly containing the controller you want to test. 

```csharp
using WebApi.Controllers;
```

Next, add a new method for each controller method you want to test (in this example, we'll only add two tests for the two `Get()` methods we talked about in the last part of this tutorial). Each test method is adorned with a `[Fact]` attribute. 

```csharp
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
```

## Running Tests

You can run these tests using the `dotnet` CLI by doing the following:

```bash
$ dotnet test
```

You can also run tests from within Visual Studio Code - notice that when you view a unit test file, Visual Studio Code provides additional options for running all tests, a single test, and for debugging tests. Clicking on the `run test` option above a test method name runs a single test in your unit test file.

Using the `debug test` option will run a test in debug mode. You can even set breakpoints, and step through your unit test code to ensure that your test is working as anticipated.

Note, this is just a basic overview of testing using [the `xunit` testing framework](https://xunit.github.io/docs/getting-started-dotnet-core).  Writing comprehensive tests is beyond the scope of this tutorial, but we'll explore more concepts related to testing in the [next part](../../tree/part-3).

## Review

In this step, we discussed:

* Writing basic unit tests for Web API controllers.
* Running test using the `dotnet` CLI and from within Visual Studio Code.

The [next part](../../tree/part-3), will cover how build a controller to access an existing REST service.
