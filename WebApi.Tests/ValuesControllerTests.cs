using System;
using Xunit;
using WebApi.Controllers;

namespace WebApi.Tests
{
    public class ValuesControllerTests
    {
        [Fact]
        public void GetMethodTest()
        {
            // Assemble
            ValuesController testController = new ValuesController();
            var expected = typeof(string[]);

            // Act
            var actual = testController.Get();

            // Asset
            Assert.IsType(expected, actual);
        }

        [Fact]
        public void GetMethodWithParamTest()
        {
            // Assemble
            ValuesController testController = new ValuesController();

            // Act
            var actual = testController.Get(5);

            // Asset
            Assert.NotNull(actual);
        }
    }
}
