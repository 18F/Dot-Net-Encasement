# .NET Core / Web API Tutorial

This part of the tutorial will cover development of a controller to access an existing legacy SOAP service.

## Part 4: Creating a SOAP Connector Class

To access a SOAP service, we'll want to use a new package from the [Nuget](https://www.nuget.org/packages/SoapHttpClient/) repository. In the integrated terminal, install the `SoapHttpClient` package (when prompted, restore dependencies after installing):

```bash
$ cd WebApi
$ dotnet add package SoapHttpClient
```

In the `Connectors` directory create a new class for a SOAP connector:

```bash
$ touch Connectors/SoapConnector.cs
```

In the new `SoapConnector.cs` file, add the following:

```csharp
using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using SoapHttpClient;
using SoapHttpClient.Extensions;
using SoapHttpClient.Enums;

namespace WebApiReferenceApp.Connectors
{
    public interface ISoapConnector
    {
        Task<string> CallServiceAsync(string method);
    }
    public class SoapConnector: ISoapConnector
    {
        private XNamespace _ns;
        private Uri _endpoint;

        public SoapConnector(XNamespace ns, Uri endpoint)
        {
            _ns = ns;
            _endpoint = endpoint;
        }

        public async Task<string> CallServiceAsync(string method)
        {
            // Construct the SOAP body
            var body = new XElement(_ns.GetName(method));

            // Make the call to the SOAP service.
            using (var soapClient = new SoapClient())
            {
                var response =
                  await soapClient.PostAsync(
                          endpoint: _endpoint,
                          soapVersion: SoapVersion.Soap11,
                          body: body);

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
```

As we did with the REST connector [in the last part](../../tree/part-3#testing-and-dependency-injection), we start by defining an interface with a single method definition. We then implement the interface in the `SoapConnector` class. We also add some private class members for the endpoint and namespace for the SOAP service.

## Creating a SOAP Controller

Create a new file in the `Controllers` directory for our SOAP controller:

```bash
$ touch Controllers/SoapController.cs
```

In the new `SoapController.cs` file, add the following content:

```csharp
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApiTutorial.Connectors;

namespace WebApiTutorial.Controllers
{
    [Route("api/[controller]")]
    public class SoapController : Controller
    {
        private ISoapConnector _connector;

        public SoapController(ISoapConnector connector)
        {
            _connector = connector;
        }

        // GET api/soap
        [HttpGet]
        public string Get()
        {
            return "Not implemented";
        }

        /**
         * Private method to invoke SoapConnector and make service call.       
        */
        private string GetSoapResponse(string methodName)
        {
            var response = _connector.CallServiceAsync(methodName);
            return response.Result;
        }
    }
}
```

Note that the public method exposed on this controller is not yet implemented. before doing so, let's write a test for this method. Initially, this test when run will fail. Implementing the public method on the `SoapController` class should cause our test to pass.

## Writing Tests

Change over to the test directory and create a new unit test file for our `SoapController` class:

```bash
$ cd ../WebApi.Tests/
$ touch SoapControllerTests.cs
```

In the new `SoapControllerTests.cs`, add the following content:

```csharp
using System.Threading.Tasks;
using Xunit;
using WebApiTutorial.Controllers;
using WebApiTutorial.Connectors;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Tests
{
    public class testSoapConnector : ISoapConnector
    {
         private string fakeResponse = @"
            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                <soap:Body>
                    <FakeResponse xmlns=""http://somefakeurl.org/"">
                    <FakeResult>
                        <string>string</string>
                    </FakeResult>
                    </FakeResponse>
                </soap:Body>
            </soap:Envelope>";

        public async Task<string> CallServiceAsync(string method)
        {            
            return await Task.FromResult(fakeResponse);
        }
    }

    public class SoapControllerTests
    {
        [Fact]
        public void GetMethodTest()
        {
            // Assemble
            ISoapConnector testConnector = new testSoapConnector();
            SoapController testController = new SoapController(testConnector);
            var expected = typeof(ContentResult);

            // Act
            var actual = testController.Get();

            // Asset
            Assert.IsType(expected, actual);

        }
    }
}
```

This file contains a unit test for the public method exposed on our SOAP controller. We set up a mock object to use for this test, to mimic the response we would get from a real SOAP service. 

When you run this test, it should fail. Next, we'll go back to our SOAP controller and implement the public method that will allow this test to pass.

## Adding Service Details

For this example, we're going to use a SOAP service from the US Geological Survey's [Grand Canyon Monitoring and Research Center](https://www.gcmrc.gov/). The details of the SOAP service can be [found here](https://www.gcmrc.gov/WebService.asmx). We're going to target the `GetLanguageList` method, which presumably just returns a list of languages.

In the `SoapControllerTests.cs` file, edit the class to add a new private member to hold the name of the method we want to invoke (note - we'll revisit this when we talk about configuration for Web API applications in a future part of this tutorial):

```csharp
private string _methodName = "GetLanguageList";
```

Now, edit the public method of the `SoapController` class to call the SOAP service and format the response:

```csharp
public ContentResult Get(string methodName)
{
    XmlDocument doc = new XmlDocument();
    doc.LoadXml(GetSoapResponse(_methodName));

    var languageList = doc.GetElementsByTagName("GetLanguageListResult");

    return Content(JsonConvert.SerializeObject(languageList), "application/json");
}
```

This logic instantiates a new [`XMLDocument` object](https://msdn.microsoft.com/en-us/library/system.xml.xmldocument(v=vs.110).aspx) and loads in the response from the SOAP service we are calling. We then access a portion of the SOAP response by using the `GetElementsByTagName` method, and then we serialize the object as JSON and return it.

When you go back to the `SoapControllerTest.cs` file we created in the previous step, you'll see that our unit test will now pass.

Because we're using dependency injection for our SOAP controller, we need to register this in the `Startup.cs` file. Open that file, and in the `ConfigureServices` section, add the following logic:

```csharp
var ns = Environment.GetEnvironmentVariable("SOAP_NAMESPACE") ?? "http://tempuri.org/";
var soap_endpoint = Environment.GetEnvironmentVariable("SOAP_ENDPOINT") ?? "https://www.gcmrc.gov/WebService.asmx";
services.AddSingleton<ISoapConnector>(new SoapConnector(XNamespace.Get(ns), new Uri(soap_endpoint)));
```

Just like with the REST Connector we created in the last part, this will register a new `SoapConnector` instance for our Web API application when it's started so that it can be used in the SOAP controller.

Now when you point your browser to `http://127.0.0.1:5000/api/soap` you'll see some content returned from the SOAP service formatted as JSON.


## Review

In this part, we discussed:

* Setting up a new SOAP Connector class that uses the [`SoapHttpClient` package](https://www.nuget.org/packages/SoapHttpClient/).
* Stubbing out a new SOAP controller, and writing unit tests.
* Modifying the SOAP controller to make calls to an existing SOAP service.
* Modifying the `Startup.cs` file to register dependency injection.


The [next part](../../tree/part-5), we'll build on these lessons and create a new controller that access a SQL Server database. 
