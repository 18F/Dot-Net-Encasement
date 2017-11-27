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
