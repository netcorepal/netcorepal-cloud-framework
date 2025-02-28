# Getting Started

## Environment Setup

### Install Development Tools

Choose one of the following development tools:

- [Visual Studio Code](https://code.visualstudio.com/)
- [Visual Studio](https://visualstudio.microsoft.com/)
- [JetBrains Rider](https://www.jetbrains.com/rider/)

### Install .NET SDK

- [.NET 9 SDK](https://dot.net)

### Install Docker Environment

Have a Docker environment for automated unit testing and integration testing.

Docker Desktop download link: (https://www.docker.com/products/docker-desktop/)

### Debug Environment Setup

This step is not mandatory. By default, automated tests will automatically start the debug environment and shut it down after the tests are completed.

The debug environment installed here is for running and debugging locally.

```shell
# redis
docker run -p 6379:6379 -d redis:7.0
# rabbitmq
docker run -p 5672:5672 -p 15672:15672  -d rabbitmq:3.9-management
# mysql
docker run -p 3306:3306 -e MYSQL_ROOT_PASSWORD=123456 -d mysql:8.0
```

The management interface address of RabbitMQ is: http://localhost:15672/, with both username and password being guest.

## Install Project Template Tool

Stable version [NetCorePal.Template - NuGet](https://www.nuget.org/packages/NetCorePal.Template)

```shell
dotnet new install NetCorePal.Template
```

or

Preview version [NetCorePal.Template - MyGet](https://www.myget.org/feed/netcorepal/package/nuget/NetCorePal.Template)

```shell
dotnet new install NetCorePal.Template::<package-version> --add-source "https://www.myget.org/F/netcorepal/api/v3/index.json"
```

## Create Project

```shell
dotnet new netcorepal-web -n MyWebApp
```

## Run Project

```shell
cd MyWebApp
# Todo: Add script to build infrastructure
dotnet run
```

## Run Tests

```shell
dotnet test
```