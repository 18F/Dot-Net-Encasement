# .NET Core / Web API Tutorial

## Introduction

This repo contains sample code and instructions for creating a RESTful API using .NET Core Web API to act as a proxy for various legacy data sources (REST API, SOAP API, PostgreSQL data base, SQL Server database). It is designed for people that have some coding experience and are comfortable building APIs that are interested in learning more [C#](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/) and [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/).

Each step in this tutorial explains how the different pieces of the Web API application are constructed, and provides an introduction to some foundational concepts for working with **C#**, **.NET Core** and **Web API**.

## Motivation

In 18F's work on legacy modernization projects, .NET and the Microsoft software stack are technologies we encounter often. These components are typically already used in the states and federal agencies we work with, which can make them a convenient choice for supporting legacy modernization efforts. But beyond the tactical advantage of using a platform that our partners are already invested in and familiar with, .NET Core and related components have a number of features that can make them a solid choice for this work. Organizational proclivity for other technologies may not make .NET an obvious choice in some instances where it might be the more optimal fit.

[ASP.NET Web API](https://www.asp.net/web-api) is a framework that can be used to create RESTful web services, which are often [central to our efforts to modernize legacy systems](https://18f.gsa.gov/2014/09/08/the-encasement-strategy-on-legacy-systems-and-the/). Building a Web API application is a good way to learn to write C# code, to become familiar with .NET Core, and to get accustomed to working with some associated tools like [Visual Studio Code](https://code.visualstudio.com/). 

## Structure

This tutorial is structured around git branches. Each step in the tutorial is contained within a distinct branch. If you are using the Github website, each step in the tutorial is linked from the `README` document in the previous step. To get the entire tutorial, and all of the related example code, just clone this repo. 

* [Introduction](https://github.com/mheadd/WebApiTutorial/tree/master)
* Part 1: [Setting up a new Web API application](https://github.com/mheadd/WebApiTutorial/tree/part-1)
* Part 2: [Creating Web API Controller Tests](https://github.com/mheadd/WebApiTutorial/tree/part-2)
* Part 3: [Creating a REST Controller](https://github.com/mheadd/WebApiTutorial/tree/part-3)
* Part 4: [Creating a SOAP Controller](https://github.com/mheadd/WebApiTutorial/tree/part-4)
* Part 5: Creating a SQL Controller (Entity Framework / SQL Server)
* Part 6: Creating a SQL Controller (PostreSQL)
* Part 7: Configuration Management in Web API.

## Getting Started

Download and install:

* [.NET Core SDK](https://dotnet.microsoft.com/download)
* [Visual Studio Code](https://code.visualstudio.com/)
* Visual Studio Code [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)

You're ready to get started. Check out [Part 1: Creating your first Web API application](../../tree/part-1).

## Further reading

These are some resources that can supplement the steps demonstrated in this tutorial and provide deeper insights into .NET Core and Web API.

### General
* [General overview](https://docs.microsoft.com/en-us/aspnet/core/tutorials/web-api-vsc) of how to create a Web API with ASP.NET Core MVC on MacOS.
* [Asynchronous programming](https://docs.microsoft.com/en-us/dotnet/csharp/async) in C#.
* [Video archive](https://channel9.msdn.com/Events/dotnetconf/2017) of .NET Conf 2017

### Testing
* [Testing controller logic](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/testing) in ASP.NET Core
* [Unit testing C# in .NET Core](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test) using `dotnet test` and xUnit 
* [Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) in ASP.NET Core

### Routing and generating responses
* [Data validation](https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api) in ASP.NET Web API applications
* Learn about the difference between [conventional routing](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing) (using routing middleware) and [attribute routing](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing#routing-mixed-ref-label) in ASP.NET MVC apps.
* Learn more about how ASP.NET Web API [converts a return value from a controller method](https://docs.microsoft.com/en-us/aspnet/web-api/overview/getting-started-with-aspnet-web-api/action-results) into an HTTP response.

### Data access / SQL Server
* [Data access](https://blogs.msdn.microsoft.com/dotnet/2016/11/09/net-core-data-access/) in .NET Core
* Use Visual Studio Code to [create and run Transact-SQL scripts for SQL Server](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-develop-use-vscode)

### Docker
* .NET Core [Docker images](https://hub.docker.com/_/microsoft-dotnet-core): Official imafges for .NET Core and ASP.NET Core.
* Overview of how to run the [SQL Server 2017 container image](https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker) in Docker
