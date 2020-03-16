dotnet ef dbcontext scaffold 'Host=localhost;Database=TestDB;Username=TestDev;Password=Xj7hgrw@hn!49' Npgsql.EntityFrameworkCore.PostgreSQL -o Models -c InspectionsContext -d

https://www.npgsql.org/efcore/


# .NET Core / Web API Tutorial

This part of the tutorial will cover development of a controller to enable access to an existing PostgreSQL database.

## Part 6: Creating a PostgreSQL Connector Class

Before we can create our new controller, we need a SQL Server database to work with. The [Docker image for SQL Server](https://hub.docker.com/_/microsoft-mssql-server) makes this easy to do. The prebuilt `Dockerfile` in the Docker directory will create a new instance of SQL Server that you can use, and populate it with some dummy data.

To use it, in the terminal pane, `cd` to the Docker directory, and edit the password placeholders in both the `Dockerfile` and `setup.sql` file. The password in the `Dockerfile` is for the SA user, which will be used to create the test database for this example, and populate it with data. If you plan on doing anything with this Docker image beyond this tutorial, you should [change the SA password](https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker?view=sql-server-ver15&pivots=cs1-bash#sapassword). The password in the `setup.sql` file is for a user account that the SQL Controller will use.

Once you've saved your changes, build the Docker image and then run it (changing the {user-name} placeholder in the example commands below).

```bash
~$ docker build -t {user-name}/sql-server-test -f Dockerfile .
~$ docker run -p 1433:1433 {user-name}/sql-server-test
```

If you have the [VS Code Docker extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-docker) installed, you should see a Docker icon on the left panel in VS Code. Clicking this will show your running image.

Now that we have a running instance of SQL Server, we can create our SQL Connector

We're going to use Entity Framework Core to access the SQL Server database. We'll need to install several packages from Nuget for this. If you have the [Nuget Package Manager extension](https://marketplace.visualstudio.com/items?itemName=jmrog.vscode-nuget-package-manager) for VS Code installed, you can find and install these packages that way. Or, you can simply use the `dotnet` CLI to install them as we've done before.

```bash
~$ dotnet add package Microsoft.EntityFrameworkCore.SqlServer
~$ dotnet add package Microsoft.EntityFrameworkCore.Tools
~$ dotnet add package Microsoft.EntityFrameworkCore.Design
```

We could write our model classes and DB context for accessing the database by hand, particularly since the sample database we'll use is not all that complex. But another option is to use Entity Framework Core tools to "reverse engineer" our database and generate models and a DB context for us.

To do this, we'll need to install Entity Framework CLI tools.

```bash
dotnet tool install --global dotnet-ef --version 3.0.0
```

Now we're ready to build our models and DB context files. In the terminal, run the following command:

```bash
~$ dotnet ef dbcontext scaffold 'Server=localhost;Database=TestDB;User Id={id};Password={your-password}' Microsoft.EntityFrameworkCore.SqlServer -o Models -c PlacesContext -d
```

Note - you'll need to replace the user id and password in the example above with the credentials you put in the `setup.sql` file previously. This command will connect to the database and generate a model for the table we created in a directory called `Models` and a DB context file for connecting to the database. Based on the name of the database table in our `setup.sql` file, the model will be named `Places.cs` and the DB context file will be named `PlacesContext.cs`

Once this is done, we'll need to make a couple of changes to the `PlacesContext.cs` file. Before the class definition in this file, we want to add a new Interface to allow us to use dependency injection as we did in previous sections.

```csharp
public interface IPlacesContext
{
    DbSet<Places> Places { get; set; }
}
```

Now, change the class definition for `PlacesContext` so that it derives from both `DBContext` and our new `IPlacesContext` Interface.

```csharp
public partial class PlacesContext : DbContext, IPlacesContext
```

In the `OnModelCreating` method of the `PlacesContext` class, change the following line:

```csharp
entity.HasNoKey();
```

To this:

```csharp
entity.HasKey(e => e.Id).HasName("PK_Id");
```
This change will come into play when we create our unit tests. 

We want our DB context to get the connection string from the `appsettings.json`, so let's add some new logic in the class constructor to do this. 

```csharp
public PlacesContext()
{
    IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json")
        .Build();
    _connectionString = configuration.GetConnectionString("PlacesDatabase");
}
```
You'll also need to add using statement at the top of the file for `Microsoft.Extensions.Configuration`.

Finally, let's remove the connection string that is hard coded into the `PlacesContext` class. Delete the `#warning` comment and move the connection string to the `appsettings.json` file like so:

```json
"ConnectionStrings": {
    "PlacesDatabase": "Server=localhost;Database=TestDB;User Id={Id};Password={your-password}"
}
```

Save these changes to the `PlacesContext.cs` file.

## Creating a SQL Controller

Following the pattern we've used previously, create a new file in the `Controllers` directory called `PlacesController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SqlApiTest.Models;
using System.Linq;

namespace SqlApiTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlacesController : ControllerBase
    {
        private IPlacesContext _context; 

        public PlacesController(IPlacesContext context) {
            _context = context;
        }
        
        [HttpGet]
        public ContentResult Get()
        {
            using PlacesContext context = new PlacesContext();
            var addresses = _context.Places;
            return Content(FormatJson(addresses), "application/json");
        }

        [HttpGet("search")]
        public ContentResult Get(string state)
        {
            using PlacesContext context = new PlacesContext();
            var addresses = _context.Places
            .Where(a => a.State == state);
            return Content(FormatJson(addresses), "application/json");
        }

        private string FormatJson(IQueryable<Places> addresses) {
            return JsonConvert.SerializeObject(addresses);
        }
    }
}

```

The second method on this class uses the [Entity Framework LINQ-to-Entities syntax](https://www.entityframeworktutorial.net/Querying-with-EDM.aspx) for querying the `places` table in our database for places by state name.

Note - because we're using dependency injection for our controller, we'll be able to easily write some unit tests in the next step. However, just like before, we need to make a small change to the `Startup.cs` file to let the framework know we're using dependency injection. Open the `Startup.cs`file, and in the `ConfigureServices` section, add the following logic:

```csharp
services.AddSingleton<IPlacesContext>(new PlacesContext());
```

Save your work, and use `dotnet run` to start your app. Now, with your test database running, you should see JSON returned at `http://127.0.0.1:3000/api/places`. You can also search for places in a specific state by using `http://127.0.0.1:3000/api/places/search?state=NY` replacing `NY` with the two character abbreviation for a US state.

## Writing Tests

Switch over to the `WebApi.Tests` directory and create a new file called `PlacesTests.cs`.

Instead of writing tests against our SQL Server instance, we're going to use the [Entity Framework in memory database](https://docs.microsoft.com/en-us/ef/core/providers/in-memory/?tabs=dotnet-core-cli) for our unit tests. To do this, we'll need to add a new package.

```bash
~$ dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

In the new `PlacesTests.cs` file, put the following content:

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
                        State = "NY" + i
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

```

The first part of this file sets up a DB context that uses the Entity Framework in memory database and populates it with some test data. We then set up two different tests to test each route handled by our controller.

Save the file and then run the tests (note - you do not need to have your Docker instance of SQL Server running to run these tests):

```bash
~$ dotnet test
```

All tests should run successfully.

## Review

In this part, we discussed:

* How to set up a test SQL Server instance using Docker.
* How to reverse engineer our SQL Server instance to generate a Model and DB context class.
* How to create a new SQL Controller class to access and serve data in SQL Server using Entity Framework.
* How to create unit tests using the Entity Framework in memory database.

In the [next part](../../tree/part-6), we'll use this same approach to explain how to access data in a legacy PostreSQL database. 
