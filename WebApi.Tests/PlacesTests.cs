using System;
using Xunit;
using WebApi.Controllers;
using WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WebApi.Tests
{
    public class PlacesControllerTests
    {
        private readonly int _counter = 10;

        private async Task<PlacesContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<PlacesContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var testContext = new PlacesContext(options);
            testContext.Database.EnsureCreated();
            if (await testContext.Places.CountAsync() <= 0)
            {
                for (int i = 1; i <= _counter; i++)
                {
                    testContext.Places.Add(new Places()
                    {
                        Id = i,
                        LatD = 41,
                        LatM = 5,
                        LatS = 59,
                        Ns = "N",
                        LonD = 80,
                        LonM = 39,
                        LonS = 0,
                        State = $"NY{i}"
                    });
                    await testContext.SaveChangesAsync();
                }
            }
            return testContext;
        }

        [Fact]
        public async Task Should_Return_All_Places()
        {
            //Arrange
            var dbContext = await GetDatabaseContext();
            var placesController = new PlacesController(dbContext);

            //Act
            var placesJSON = placesController.Get();
            var places = JArray.Parse(placesJSON.Content);
            var count = places.Count;

            //Assert
            Assert.Equal(_counter, count);
        }

        [Fact]
        public async Task Should_Only_Return_Places_In_State_Searched()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var placesController = new PlacesController(dbContext);

            // Act
            var placesJSON = placesController.Get("NY1");
            var places = JArray.Parse(placesJSON.Content);
            var count = places.Count;

            // Assert
            Assert.Equal(1, count);
        }
    }
}
