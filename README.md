# .NET Core / Web API Tutorial

This part of the tutorial will cover development of a controller to access an existing legacy REST service.

## Part 2: Creating a REST Controller

To demonstrate how to build a service that can sit in front of an existing REST service, we'll use the API's built into the [Data.gov site](https://www.data.gov/). This site is built on the open source CKAN platform, so it's APIs are [well documented](http://docs.ckan.org/en/latest/api/) and well understood.

We'll create a controller that can be used to query the Data.gov site, and return a portion of the raw response from the CKAN API. This might match a use case you would encounter if you had an existing legacy service that you wanted to query and return only part of, or if you wanted to query an existing service and combine part of the response with data from another service or backend system.

Navigate to the `WebApi` directory and create a new controller called `RestController.cs`:

```bash
$ cd WebApi
$ touch Controllers/RestController.cs
```

Open the integrated terminal and install a new package:

```bash
$ dotnet add package Newtonsoft.Json
```

When promoted in Visual Studio Code, restore dependencies. Otherwise, do `dotnet restore` in the integrated terminal.

As we saw in the previous part, the WebAPI framework will automatically render .NET objects as JSON when generating a response, but we want to use the [Json.Net package](https://www.newtonsoft.com/json) to manipulate and change the response from the Data.gov CKAN API. (We'll also use this package to generate JSON from other types of backend systems in future parts of this tutorial.)

Add the following to your `RestController.cs` file:

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace WebApiReferenceApp.Controllers
{
    [Route("api/[controller]")]
    public class RestController : Controller
    {
        // Data.gov API endpoint.
        private Uri endpoint = new Uri("https://catalog.data.gov");
        // Path to list packages.
        private string packageSearch = "/api/3/action/package_search";
        // Path to show package details.
        private string packageDetails = "/api/3/action/package_show?id={0}";

        // GET api/rest/search
        [HttpGet("search")]
        public ContentResult Get()
        {
            string response = GetPackageData(packageSearch);
            return Content(response, "application/json");
        }

        // GET api/rest/details?id=
        [HttpGet("details")]
        public ContentResult Get(string id)
        {
            string path = String.Format(packageDetails, id);
            string response = GetPackageData(path);
            return Content(response, "application/json");
        }

        private string GetPackageData(string path)
        {
            JObject result = JObject.Parse(CallServiceAsync(path).Result);
            return result.GetValue("result").ToString();
        }

        private async Task<string> CallServiceAsync(string path)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = endpoint;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    return await client.GetStringAsync(path);
                }
                catch (HttpRequestException ex)
                {
                    return String.Format(@"{{""result"": {{""error"": ""{0}""}}}}", ex.Message);
                }
            }
        }
    }
}
```

This file has three private class members that hold the URL and paths for the REST calls we want to make. There are two public methods for listing packages in the Data.gov site, and for showing the details of a specific package. Both methods use [attribute routing](../..//tree/part-1#modifying-your-new-web-api-application) as discussed in previous section.

We use a private method to make the API call to the Data.gov API. This private method uses the [C# `HttpClient` class](https://msdn.microsoft.com/en-us/library/system.net.http.httpclient(v=vs.118).aspx) and is structured to take advantage of .NET Core's support for [asynchronous programming](https://docs.microsoft.com/en-us/dotnet/csharp/async).

Save your changes, then you should be able to point your browser to `http://localhost:5000/api/rest/search` and see the JSON response returned by the Data.gov API. The second public method in this file takes an `id` parameter - this is the package ID for the resource provided by CKAN. We can look up the details of a specific CKAN package on Data.gov by pointing our browser to `http://localhost:5000/api/rest/details?id=000f1c44-a0b8-402f-8d4b-a4b66dfb7734`. 

## Testing and Dependency Injection

This REST controller works fine as it is, but if we want to write tests for this controller things get more complicated. We want to write tests that ensure that the controller returns the correct response type, not whether the Data.gov API is responding to requests. As things are we can't do this right now. To make this possible, we need to introduce [dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) into our controller.

In the `WebApi` directory, create a new subdirectory called `Connectors` - we'll use this for classes that hold the logic needed to make connections to various backend systems. Create a new file in this subdirectory called `RestConnector.cs`.

```bash
$ mkdir Connectors
$ touch Connectors/RestConnector.cs
```

In the new `RestConnector.cs` file, add the following logic.

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WebApiTutorial.Connectors
{
    public interface IRestConnector
    {
        Task<string> CallServiceAsync(string path);
    }
    public class RestConnector : IRestConnector
    {
        private Uri _endpoint;
        public RestConnector(Uri endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task<string> CallServiceAsync(string path)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = _endpoint;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    return await client.GetStringAsync(path);
                }
                catch (HttpRequestException ex)
                {
                    return String.Format(@"{{""result"": {{""error"": ""{0}""}}}}", ex.Message);
                }
            }
        }
    }
}
```

This file contains a new [interface](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/interface) - `IRestConnector` - as well as a class that implements from this interface. This derived class will be used to make API calls for our REST controller.

Now we need to modify our `RestController.cs` to use this new connector.

```csharp
using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WebApiTutorial.Connectors;

namespace WebApiTutorial.Controllers
{
    [Route("api/[controller]")]
    public class RestController : Controller
    {
        // Path to list packages.
        private string packageSearch = "/api/3/action/package_search";
        // Path to show package details.
        private string packageDetails = "/api/3/action/package_show?id={0}";

        private IRestConnector _connector;

        public RestController(IRestConnector connector)
        {
            _connector = connector;
        }

        // GET api/rest/search
        [HttpGet("search")]
        public ContentResult Get()
        {
            string response = GetPackageData(packageSearch);
            return Content(response, "application/json");
        }

        // GET api/rest/details?id=
        [HttpGet("details")]
        public ContentResult Get(string id)
        {
            string path = String.Format(packageDetails, id);
            string response = GetPackageData(path);
            return Content(response, "application/json");
        }

        /**
         * Private method to invoke RestConnector and make API call.       
         */
        private string GetPackageData(string path)
        {
            // Make the API call using the psecificed path.
            var response = _connector.CallServiceAsync(path).Result;

            // Parse the raw API response.
            JObject result = JObject.Parse(response);

            // Return the result.
            return result.GetValue("result").ToString();
        }
    }
}
```

Notice that we've added a new `using` statement for our connector class. We've also added a private class member of type `IRestConnector`, and added a parameter to the class constructor that requires a connector class to be passed in when instantiated (thereby injecting the dependency into the class).

## Writing Tests

Before proceeding further, let's write some tests for our new controller. Change to the `WebApi.Tests` directory and create a new test file for our REST controller:

```bash
$ cd ../WebApi.Tests/
$ touch RestControllerTests.cs
```

In the new `RestControllerTests.cs` file, add the following content:

```csharp
using System.Threading.Tasks;
using Xunit;
using WebApiTutorial.Controllers;
using WebApiTutorial.Connectors;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Tests
{
    // A test connector to use for our tests.
    public class testConnector : IRestConnector
    {
        public async Task<string> CallServiceAsync(string path)
        {
            return await Task.FromResult(@"{""result"":{""foo"": ""bar""}}");
        }
    }

    public class RestControllerTests
    {
        [Fact]
        public void GetMethodTest()
        {
            // Assemble
            IRestConnector testConnector = new testConnector();
            RestController testController = new RestController(testConnector);
            var expected = typeof(ContentResult);

            // Act
            var actual = testController.Get();

            // Asset
            Assert.IsType(expected, actual);
        }

        [Fact]
        public void GetMetodWithIdTest()
        {
            // Assemble
            IRestConnector testConnector = new testConnector();
            RestController testController = new RestController(testConnector);
            var expected = typeof(ContentResult);

            // Act
            var actual = testController.Get("000f1c44-a0b8-402f-8d4b-a4b66dfb7734");

            // Asset
            Assert.IsType(expected, actual);
        }
    }
}
```

At the top of the file, we set up a simple utility object that implements the `IRestConnector` interface. This mock object will allow us to ensure that our REST controller is returning the proper response in a way that isn't tied directly to the Data.gov API.

Next, we create a test method for each public method in our REST controller. For each test, we instantiate an instance of our mock connector, and use it to set up our controller test. Each test is constructed using the assemble / act / assert construct we used in the [last part of this tutorial](../../tree/part-2#part-2-creating-web-api-controller-tests) when testing was discussed.

If you run `dotnet test`, all tests should pass.

Creating comprehensive tests for asynchronous logic is beyond the scope of this tutorial, but there are some [good resources available](https://msdn.microsoft.com/en-us/magazine/dn818493.aspx) for diving more deeply into this topic. In addition to the approach discussed here, you can also use [testing frameworks](https://www.nuget.org/packages/Moq/) to create mock objects for use in your asynchronous tests.

## Registering Dependency Injection

Now that we've created a separate connector class for our legacy REST API, and added dependency injection to our controller, we need to register our use of dependency injection with the Web API framework. Change to the `WebApi` directory and open the `Startup.cs` file. 

Add a new using statement for the `Connectors` namespace.

```csharp
using WebApiTutorial.Connectors;
``` 

In the `ConfigureServices` method, add the following:

```csharp
var rest_endpoint = Environment.GetEnvironmentVariable("REST_URI") ?? "https://catalog.data.gov";
services.AddSingleton<IRestConnector>(new RestConnector(new Uri(rest_endpoint)));
```

This will create a new variable for the endpoint to use in the REST controller getting the value from an environmental variable called `REST_URI` (if it exists) or the string `https://catalog.data.gov`. We then use this variable to a create new RestConnector instance and register it with the WebAPI framework via the `services.AddSingleton` method.

So now, when you point your web browser at `http://localhost:5000/api/rest/search` you'll see the expected JSON response.

## Review

In this part, we discussed:

* Setting up a new controller.
* Adding a new package via the `dotnet` CLI.
* Asynchronous programming in C#.
* Writing loosely coupled code and dependency injection in Web API.
* Writing tests using a mock object.
* Registering a new service in Web API framework.

The [next part](../../tree/part-4), we'll build on these lessons and create a new controller and tests for a legacy SOPA-based service.