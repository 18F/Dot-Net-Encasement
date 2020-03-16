using System;
using Xunit;
using WebApi.Controllers;
using WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WebApi.Tests
{
    public class InspectionsControllerTests
    {
        private readonly int _counter = 10;

        private async Task<InspectionsContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<InspectionsContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var testContext = new InspectionsContext(options);
            testContext.Database.EnsureCreated();
            if (await testContext.Inspections.CountAsync() <= 0)
            {
                for (int i = 1; i <= _counter; i++)
                {
                    testContext.Inspections.Add(new Inspections()
                    {
                        ScoreRecent = 100,
                        GradeRecent =  $"A{i}",
                        Grade2 = "A",
                        Grade3 = "A",
                        PermitNumber = i,
                        FacilityType = 605,
                        FacilityTypeDescription = "FOOD SERVICE ESTABLISHMENT",
                        Subtype = 33,
                        SubtypeDescription = "SCHOOL CAFETERIA OR FOOD SERVICE",
                        PremiseName = "SUPER COOL FOOD PLACE",
                        PremiseAddress = "9620 WESTPORT RD",
                        PremiseCity = "LOUISVILLE",
                        PremiseState = "KY",
                        PremiseZip = 40241,
                        OpeningDate = new DateTime()
                    });
                    await testContext.SaveChangesAsync();
                }
            }
            return testContext;
        }

        [Fact]
        public async Task Should_Return_All_Inspections()
        {
            //Arrange
            var dbContext = await GetDatabaseContext();
            var inspectionsController = new InspectionsController(dbContext);

            //Act
            var inspectionsJSON = inspectionsController.Get();
            var inspections = JArray.Parse(inspectionsJSON.Content);
            var count = inspections.Count;

            //Assert
            Assert.Equal(_counter, count);
        }

        [Fact]
        public async Task Should_Only_Return_Inspections_With_Grade_Searched()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var inspectionsController = new InspectionsController(dbContext);

            // Act
            var inspectionsJSON = inspectionsController.Get("A1");
            var inspections = JArray.Parse(inspectionsJSON.Content);
            var count = inspections.Count;

            // Assert
            Assert.Equal(1, count);
        }
    }
}