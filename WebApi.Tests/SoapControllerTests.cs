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