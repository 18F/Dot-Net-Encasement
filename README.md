# .NET Core / Web API Tutorial

## Part 1: Setting up a new Web API application

Open a terminal, and Navigate to a new directory. Create a set of parallel directories to hold your new Web API application and tests:

```bash
$ mkdir -p WebApiTest/{WebApi,WebApi.Tests}
```

Navigate to the `WebApi` subdirectory and create a new Web API application using the [.NET Core command line interface](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x) (CLI):

```bash
$ cd WebApiTest/WebApi
$ dotnet new webapi
```

This will bootstrap a simple Web API application that you can work with. Note - there are other tools that you can use to bootstrap a new Web API application. In particular, `Yeoman` is a tool worth [checking out](http://jakeydocs.readthedocs.io/en/latest/client-side/yeoman.html).

You can continue to use your open terminal, or you can use the integrated terminal view in Visual Studio Code (open using the `control` + `` ` `` keys, or from the top menu via `View` > `Integrated Terminal`).

Make sure you can run your new Web API application:

```bash
$ dotnet run
```

Open a web browser and point to `https://localhost:5001/WeatherForecast` and you should see some JSON.

Note - you may have to change the local port on which this application is served. By default, the [Kestral](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-2.2) web server that ships with .NET Core serves WebAPI apps on ports `5000` and `5001`. If you have trouble accessing these ports for some reason, you can change it by updating the values in the `Properties/launchSettings.json` file, or you can update the `Program.cs` file to change this line:

```csharp
.UseStartup<Startup>();
```

to this:

```csharp
.UseStartup<Startup>()
.UseUrls("http://*:3000");
```

This will serve your app on `http://localhost:3000`.

## Modifying your new Web API application

In the `Controllers` directory, open the file `WeatherForecastController.cs`. This is an example file created by the .NET Core CLI tool that renders the JSON you just saw in your browser. Notice the route attribute on the `WeatherForecastController` class:

```csharp
[Route("[controller]")]
```

This attribute routes requests to your Web API application using the name of the controller (without the `Controller` part). So requests to `http://{host}/WeatherForecast` will be handled by this controller. Attribute routing isn't the only way to route requests in a Web API application, but this approach does have [some advantages](https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2#why-attribute-routing) when building RESTful APIs.

Try changing the route attribute to:

```csharp
[Route("api/[controller]")]
```

After saving and restarting your application, this will change the path that routes to this controller to `http://localhost:3000/api/WeatherForecast`. You can further define how specific routes are handled by using attributes on specific methods. For example, Let's create a additional `Get()` method for this class that accepts a parameter:

```csharp
[HttpGet("length/{length}")]
public IEnumerable<WeatherForecast> Get(int length)
{
    int range = length >  Summaries.Length ? Summaries.Length : length;
    var rng = new Random();
    return Enumerable.Range(1, range).Select(index => new WeatherForecast
    {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = rng.Next(-20, 55),
        Summary = Summaries[rng.Next(Summaries.Length)]
    })
    .ToArray();
}
```

Web API will [bind parameters to variables](https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/parameter-binding-in-aspnet-web-api) so that we can access parameter values inside methods. The changes we have made will match GET requests for routes with a `length` value to this specific method. We can access the `length` parameter as an integer inside our method and use it to change the result that is returned.

Save and restart your application. Try accessing `http://localhost:3000/api/WeatherForecast/length/4` in your browser. You'll see only 4 weather forecasts returned.

## Setting up watcher

You may have noticed that we stopped and restarted our application several times as we made our changes. As you work on your Web API application, this may become more of an inconvenience. To automatically restart your application when changes are made, we'll need to [set up `dotnet watch`](https://docs.microsoft.com/en-us/aspnet/core/tutorials/dotnet-watch?view=aspnetcore-3.1).

In your terminal, add a new package to install `watch`:

```bash
$ dotnet add package Microsoft.DotNet.Watcher.Tools
```

You can now invoke the watcher tool when running your Web API app like this:

```bash
$ dotnet watch run
```

Now, when we change our method, the watcher will automatically pick up the change to the `WeatherForecastController.cs` file and restart your application. To demonstrate, make the following changes to the `ValuesController.cs` file and save your changes.

```csharp
int range = length >  Summaries.Length ? Summaries.Length : length-1;
```

With watcher running, you can see these changes in your browser without manually restarting your Web API application. Now when you access `http://localhost:3000/api/WeatherForecast/length/4` only 3 weather forecasts are returned.

## Adding CORS support

Now that we understand how requests are routed, how parameters are bound, and how value are returned, let's add some additional functionality that will be needed by our Web API application.

Some Web API applications contain a directory called `wwwroot`. This is typically where [static resources are stored](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files). We're not going to be using static files that access the RESTful API we're creating - instead, we're going to create an API that can serve requests from other hosts. To do this, we'll need to enable [CORS](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS).

Open the `Startup.cs` file and add the following to the `ConfigureServices` method before the call to `services.AddMvc();`:

```csharp
services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
            builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
    });
```

This defines a CORS policy that your Web API app will use when handling requests. Next, in the `Configure` method, add the following before the call to `app.UseMvc()`:

```csharp
 app.UseCors("CorsPolicy");
```

Now, when your app responds to requests from external hosts, it should send the appropriate CORS headers:

```bash
~$ curl -v -H 'Origin: http://some-fake-host.com' http://localhost:3000/api/WeatherForecast/length/4
*   Trying ::1...
* TCP_NODELAY set
* Connected to localhost (::1) port 3000 (#0)
> GET /api/WeatherForecast/length/4 HTTP/1.1
> Host: localhost:3000
> User-Agent: curl/7.54.0
> Accept: */*
> Origin: http://some-fake-host.com
> 
< HTTP/1.1 200 OK
< Date: Sat, 15 Feb 2020 21:10:31 GMT
< Content-Type: application/json; charset=utf-8
< Server: Kestrel
< Transfer-Encoding: chunked
< Access-Control-Allow-Origin: *
< 
* Connection #0 to host localhost left intact
[{"date":"2020-02-16T16:10:31.921169-05:00","temperatureC":52,"temperatureF":125,"summary":"Freezing"},{"date":"2020-02-17T16:10:31.921198-05:00","temperatureC":-19,"temperatureF":-2,"summary":"Bracing"},{"date":"2020-02-18T16:10:31.921208-05:00","temperatureC":39,"temperatureF":102,"summary":"Scorching"}]
```

## Review

In this step, we discussed:

- Attribute routing
- Parameter binding
- Adding `dotnet watch` to a project
- Modifying the `Startup.cs` file to add CORS support.

In the [next part](../../tree/part-2), we'll cover how to set up tests for Web API controllers.
