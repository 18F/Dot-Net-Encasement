# .NET Core / Web API Tutorial

This part of the tutorial will cover development of a controller to enable access to an existing PostgreSQL database.

## Part 6: Creating a PostgreSQL Connector Class

Before we can create our new controller, we need a SQL Server database to work with. We'll use the official PostgreSQL Docker image for this. The prebuilt `Dockerfile` in the `WebApi/Docker/Postgres` directory will create a new instance of PostgreSQL that you can use, and populate it with some dummy data.

Use the same commands to build and run the Docker image as we used in the last section:

```bash
~$ docker build -t {user-name}/sql-server-test -f Dockerfile .
~$ docker run -p 1433:1433 {user-name}/sql-server-test
```

Now that we have a running instance of PostgreSQL, we can create our SQL Connector and object model.

We're going to reuse Entity Framework Core to access the PostgreSQL database, just ass we did in the last section. We'll need to install another package from Nuget for this. If you have the [Nuget Package Manager extension](https://marketplace.visualstudio.com/items?itemName=jmrog.vscode-nuget-package-manager) for VS Code installed, you can find and install these packages that way. Or, you can simply use the `dotnet` CLI to install them as we've done before.

```bash
~$ dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

Just s we did in the lat section, we'll use Entity Framework Core tools to "reverse engineer" our database and generate models and a DB context for us.

```bash
~$ dotnet ef dbcontext scaffold 'Host=localhost;Database=TestDB;Username={user};Password={your-password}' Npgsql.EntityFrameworkCore.PostgreSQL -o Models -c InspectionsContext -d
```

Note - you'll need to replace the user id and password in the example above with the credentials in the PostgreSQL Docker file. This command will connect to the database and generate a model for the table we created in a directory called `Models` and a DB context file for connecting to the database. Based on the name of the database table in our `setup.sql` file, the model will be named `Inspections.cs` and the DB context file will be named `InspectionsContext.cs`

Once this is done, we'll need to make a couple of changes to the `InspectionsContext.cs` file. Before the class definition in this file, we want to add a new Interface to allow us to use dependency injection as we did in previous sections.

```csharp
public interface IInspectionsContext
{
    DbSet<Inspections> Inspections { get; set; }
}
```

Now, change the class definition for `InspectionsContext` so that it derives from both `DBContext` and our new `IInspectionsContext` Interface.

```csharp
public partial class InspectionsContext : DbContext, IInspectionsContext
```

In the `OnModelCreating` method of the `InspectionsContext` class, change the following line:

```csharp
entity.HasNoKey();
```

To this:

```csharp
entity.HasKey(e => e.PermitNumber).HasName("PK_PermitNumber");
```
This change will come into play when we create our unit tests. 

We want our DB context to get the connection string from the `appsettings.json`, so let's add some new logic in the class constructor to do this. 

```csharp
public InspectionsContext()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json")
        .Build();
        _connectionString = configuration.GetConnectionString("InspectionsDatabase");
    }
```
You'll also need to add using statement at the top of the file for `Microsoft.Extensions.Configuration`.

Finally, let's remove the connection string that is hard coded into the `InspectionsContext` class. Delete the `#warning` comment and move the connection string to the `appsettings.json` file like so:

```json
"ConnectionStrings": {
    "PlacesDatabase": "Server=localhost;Database=TestDB;User Id={Id};Password={your-password}",
    "InspectionsDatabase": "Host=localhost;Database=TestDB;Username={user};Password={your-password}"
}
```

Save these changes to the `InspectionsContext.cs` file.

## Creating a SQL Controller

Following the pattern we've used previously, create a new file in the `Controllers` directory called `InspectionsController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Models;
using System.Linq;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionsController : ControllerBase
    {
        private IInspectionsContext _context;

        public InspectionsController(IInspectionsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ContentResult Get()
        {
            var inspections = _context.Inspections;
            return Content(FormatJson(inspections), "application/json");
        }

        [HttpGet("search")]
        public ContentResult Get(string grade)
        {
            var inspections = _context.Inspections
            .Where(a => a.GradeRecent == grade);
            return Content(FormatJson(inspections), "application/json");
        }

        private string FormatJson(IQueryable<Inspections> inspections)
        {
            return JsonConvert.SerializeObject(inspections);
        }
    }
}
```

The second method on this class uses the [Entity Framework LINQ-to-Entities syntax](https://www.entityframeworktutorial.net/Querying-with-EDM.aspx) for querying the `inspections` table in our database for inspections by inspection grade.

Just like before, we need to make a small change to the `Startup.cs` file to let the framework know we're using dependency injection. Open the `Startup.cs`file, and in the `ConfigureServices` section, add the following logic:

```csharp
services.AddSingleton<IInspectionsContext>(new InspectionsContext());
```

Save your work, and use `dotnet run` to start your app. Now, with your test database running, you should see JSON returned at `http://127.0.0.1:3000/api/inspections`. You can also search for inspections with a specific grade by using `http://127.0.0.1:3000/api/inspections/search?grade=C` replacing `C` with the letter grade of an inspection result that you want to view.

## Writing Tests

Switch over to the `WebApi.Tests` directory and create a new file called `InspectionsControllerTests.cs`.

Just as we did in the last section, instead of writing tests against our PostgreSQL instance, we're going to use the [Entity Framework in memory database](https://docs.microsoft.com/en-us/ef/core/providers/in-memory/?tabs=dotnet-core-cli) for our unit tests. 

In the new `InspectionsControllerTests.cs` file, put the following content:

```csharp
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

```

The first part of this file sets up a DB context that uses the Entity Framework in memory database and populates it with some test data. We then set up two different tests to test each route handled by our controller.

Save the file and then run the tests (note - you do not need to have your Docker instance of PostgreSQL running to run these tests):

```bash
~$ dotnet test
```

All tests should run successfully.

## Review

In this part, we discussed:

* How to set up a test PostgreSQL instance using Docker.
* How to reverse engineer our PostgreSQL instance to generate a Model and DB context class.
* How to create a new SQL Controller class to access and serve data in PostgreSQL using Entity Framework.
* How to create unit tests using the Entity Framework in memory database.

In the [next part](#), we'll div e more deeply into conficuration management for WebAPI applications. 
