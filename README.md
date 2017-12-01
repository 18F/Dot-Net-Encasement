# .NET Core / Web API Tutorial

## Part 1: Setting up a new Web API application

Open a terminal, and Navigate to a new directory. Create a set of parallel directories to hold your new Web API application and tests:

```bash
$ mkdir WebApi WebApi.Tests
```

Navigate to the `WebApi` directory and create a new Web API application using the [.NET Core command line interface](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x) (CLI):

```bash
$ cd WebApi
$ dotnet new webapi
```

This will bootstrap a simple Web API application that you can work with. You can continue to use your open terminal, or you can use the integrated terminal view in Visual Studio Code (open using the **control** + **\`** keys, or from the top menu via `View` > `Integrated Terminal`).

Make sure you can run your new Web API application:

```bash
$ dotnet run
```

Open a web browser and point to `http://localhost:5000/api/values` and you should see some JSON.

## Modifying your new Web API application

In the `Controllers` directory, open the file `ValuesController.cs`. This is an example file created by the .NET Core CLI tool that renders the JSON you just saw in your browser. Notice the route attribute on the `ValuesController` class:

```csharp
[Route("api/[controller]")]
```

This attribute routes requests to your Web API application using the path `/api` + the name of the controller (without the "Controller" part). So requests to `http://{host}/api/values` will be handled by this controller. Attribute routing isn't the only way to route requests in a Web API application, but this approach does have [some advantages](https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2#why-attribute-routing) when building RESTful APIs.

You can further define how specific routes are handled by using attributes on specific methods. For example, notice the attribute used on one of the Get methods:

```csharp
[HttpGet("{id}")]
public string Get(int id)
{
    return "value";
}
```

This attribute matches GET requests for routes with an `id` value to this specific method. Try accessing `http://localhost:5000/api/values/5` in your browser. You'll see the string "value" returned. 

We can change the path that will match this method by altering the `HttpGet` attribute. Make the following change, save it, and restart your application:

```csharp
[HttpGet("value/{id}")]
public string Get(int id)
{
    return "value";
}
```

Now this method is invoked when you access `http://localhost:5000/api/values/value/5` in your browser. In addition, Web API will [bind parameters to variables](https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/parameter-binding-in-aspnet-web-api) so that we can access parameter values inside methods. Instead of just returning the string "value" from our method, let's return the value in the `id` variable. Make the following change, save it,  and restart your application:

```csharp
[HttpGet("value/{id}")]
public Object Get(int id)
{
    return new { value = id }; 
}
```

Now, access `http://localhost:5000/api/values/value/5` in your web browser. Since we changed the return type of the method from `string` to `Object` we can use object literal notation in C# to return a new anonymous type. Web API will automatically convert this return value to JSON when it renders the output. Try looking at the **Content-type** header in the response when you access the same route:

```bash
$ curl -v http://localhost:5000/api/values/value/5
*   Trying ::1...
* TCP_NODELAY set
* Connected to localhost (::1) port 5000 (#0)
> GET /api/values/value/5 HTTP/1.1
> Host: localhost:5000
> User-Agent: curl/7.54.0
> Accept: */*
> 
< HTTP/1.1 200 OK
< Date: Fri, 24 Nov 2017 14:28:46 GMT
< Content-Type: application/json; charset=utf-8
< Server: Kestrel
< Transfer-Encoding: chunked
< 
* Connection #0 to host localhost left intact
{"value":5}
```

## Setting up watcher

You may have noticed that we stopped and restarted our application several times as we made these changes. As you work on your Web API application, this may become more of an inconvenience. To automatically restart your application when changes are made, we'll need to [set up `dotnet watch`](https://github.com/aspnet/DotNetTools/tree/dev/src/Microsoft.DotNet.Watcher.Tools#how-to-install).

Open the file `WebApi.csproj` and add a new `<DotNetCliToolReference/>` item for the watcher utility:

```xml
<ItemGroup>
    <DotNetCliToolReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Tools" Version="2.0.0" />
    <DotNetCliToolReference Include="Microsoft.DotNet.Watcher.Tools" Version="2.0.0" />
</ItemGroup>
```

In your integrated terminal type `dotnet restore`. You can now invoke the watcher tool when running your Web API app like this:

```bash
$ dotnet watch run
```

Now, when we change our method, the watcher will automatically pick up the change to the `ValuesController.cs` file and restart your application. To demonstrate, make the following changes to the `ValuesController.cs` file and save your changes.

```csharp
[HttpGet("value/{id}")]
public Object Get(int id)
{
    return new { value1 = id , value2 = (id*2)}; 
}
```

With watcher running, you can see these changes in your browser without manually restarting your Web API application.

## Adding CORS support

Now that we understand how requests are routed and how parameters are bound and values returned, let's add some additional functionality that will be needed by our Web API application. 

You may have noticed that when we bootstrapped our Web API application, a directory called `wwwroot` was created. This is typically where [static resources are stored](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files). We're not going to be using static files that access the RESTful API we're creating - instead, we're going to create an API that serves requests from other hosts. To do this, we'll need to enable [CORS](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS). To do this, we need to modify the file `Startup.cs`.

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
$ curl -v -H 'Origin: http://some-fake-host.com' http://localhost:5000/api/values/value/5
*   Trying ::1...
* TCP_NODELAY set
* Connected to localhost (::1) port 5000 (#0)
> GET /api/values/value/5 HTTP/1.1
> Host: localhost:5000
> User-Agent: curl/7.54.0
> Accept: */*
> Origin: http://some-fake-host.com
> 
< HTTP/1.1 200 OK
< Date: Fri, 24 Nov 2017 15:17:08 GMT
< Content-Type: application/json; charset=utf-8
< Server: Kestrel
< Transfer-Encoding: chunked
< Vary: Origin
< Access-Control-Allow-Credentials: true
< Access-Control-Allow-Origin: http://some-fake-host.com
< 
* Connection #0 to host localhost left intact
{"value1":5,"value2":10}
```

## Review

In this step, we discussed:

* Attribute routing
* Parameter binding
* Adding `dotnet watch` to a project
* Modifying the `Startup.cs` file to add CORS support.

In the [next part](../../tree/part-2), we'll cover how to set up tests for Web API controllers.
