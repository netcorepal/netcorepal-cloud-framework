# netcorepal-cloud-framework　　　　　　　　　　[中文](https://github.com/netcorepal/netcorepal-cloud-framework/blob/main/README.md)

[![Release Build](https://img.shields.io/github/actions/workflow/status/netcorepal/netcorepal-cloud-framework/release.yml?label=release%20build)](https://github.com/netcorepal/netcorepal-cloud-framework/actions/workflows/release.yml)
[![Preview Build](https://img.shields.io/github/actions/workflow/status/netcorepal/netcorepal-cloud-framework/dotnet.yml?label=preview%20build)](https://github.com/netcorepal/netcorepal-cloud-framework/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/NetCorePal.Extensions.AspNetCore.svg)](https://www.nuget.org/packages/NetCorePal.Extensions.AspNetCore)
[![MyGet Preview](https://img.shields.io/myget/netcorepal/vpre/NetCorePal.Extensions.AspNetCore?label=preview)](https://www.myget.org/feed/netcorepal/package/nuget/NetCorePal.Extensions.AspNetCore)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/netcorepal/netcorepal-cloud-framework/blob/main/LICENSE)

A `tactical` framework for `Domain-Driven Design` based on `ASP.NET Core`.

Core features:
+ Domain-Driven Design practice support
+ CQRS
+ Event Driven
+ Distributed transactions (eventual consistency of event handling)
+ Multi-tenant
+ Multi-environment (canary release)

## How to use

### Using the template tool

Create a project using the `NetCorePal.Template` template tool:

```cmd
# Install the template tool
dotnet new -i NetCorePal.Template
# Create a project
dotnet new netcorepal-web -n My.Project.Name
```

Template tool source code: <https://github.com/netcorepal/netcorepal-cloud-template>

### Quick start documentation

+ [Create a project](https://netcorepal.github.io/netcorepal-cloud-framework/en/getting-started/getting-started/)
+ [Project structure](https://netcorepal.github.io/netcorepal-cloud-framework/en/getting-started/project-structure/)
+ [Development process](https://netcorepal.github.io/netcorepal-cloud-framework/en/getting-started/development-process/)

### Complete documentation

<https://netcorepal.github.io/netcorepal-cloud-framework/en>

## Vision

With the development of the .NET technology ecosystem, its microservice architecture in the cloud-native era has become very mature, and the implementation of Domain-Driven Design has also received very good support. At the same time, with the increasing demand for informatization and digitalization in various industries, more enterprises and teams are beginning to pay attention to how to effectively organize R&D teams and realize their business architecture.

The core purpose of this project is to help enterprises quickly build a technical implementation framework based on Domain-Driven Design, while accumulating and discussing the methodology of Domain-Driven Design, so that more enterprises and teams can benefit.

## Roadmap

Planned capabilities

+ [x] Support for flexible configuration and deployment of gateways
+ [x] Rapid development framework based on `ASP.NET Core` and open-source components
+ [x] Provide code template project scaffolding for Domain-Driven Design implementation
+ [x] Implement a business-extensible overall canary release solution
+ [x] Implement business-extensible tenant capabilities
+ [x] Microservice architecture practice based on Domain-Driven Design
+ [x] Modular design, can be used and replaced as needed
+ [x] Provide detailed documentation
+ [x] Provide microservice infrastructure with a visual operation interface
    + [x] Based on .NET Aspire

## Component description

+ [x] Context Passing
    + [x] AspNetCore (HTTP Request)
    + [x] HttpClient
    + [x] RabbitMQ (Based on DotNetCore.CAP)
+ [x] Domain
    + [x] Entity
    + [x] StronglyTypedId (With Source Generator)
    + [x] AggregateRoot
    + [x] DomainEvent
+ [x] Repository (Based On EntityFrameworkCore)
+ [x] Transaction
    + [x] UnitOfWork
    + [x] Distributed Transaction
        + [x] Outbox (Based on DotNetCore.CAP)
+ [x] IdGeneration
    + [x] Snowflake
        + [x] Snowflake with Etcd
        + [x] Snowflake with Redis
        + [x] Snowflake with Consul
+ [x] Primitives
    + [x] Exception Handling
    + [x] Clock
+ [x] Service Discovery
    + [x] Microsoft Service Discovery (Aspire)
    + [x] Service Discovery Kubernetes
+ [x] Multi Tenant
+ [x] Multi Environment
    + [x] Gray Environment

For more information, please refer to the [documentation](docs/index.md)

## Referenced projects

+ [AspNetCore](https://github.com/dotnet/aspnetcore)
+ [EntityFrameworkCore](https://github.com/dotnet/efcore)
+ [MediatR](https://github.com/jbogard/MediatR)
+ [DotNetCore.CAP](https://github.com/dotnetcore/CAP)
+ [KubernetesClient](https://github.com/kubernetes-client/csharp)
+ [DistributedLock.Redis](https://github.com/madelson/DistributedLock)

## About collaboration

We have an open mindset and welcome anyone to provide opinions and suggestions, and also welcome anyone to contribute code.

## Development and debugging

1. Install `.NET 9.0 SDK` or higher.

   SDK download address: <https://dot.net/download>

2. Have a `Docker` environment for automated unit testing and integration testing.

   `Docker Desktop` download address: <https://www.docker.com/products/docker-desktop/>

3. Build the project

    ```shell
    dotnet build
    ```

4. Run tests

    ```shell
    dotnet test
    ```

5. Other optional dependencies

   Install `skywalking`

    ```shell
    # Install oap
    docker run --name oap -p 11800:11800 -p 12800:12800 --restart always -d apache/skywalking-oap-server:9.0.0

    # Install oap-ui
    export version=9.0.0
    docker run --name oap-ui -p 8080:8080 -d --link oap -e SW_OAP_ADDRESS=http://oap:12800  apache/skywalking-ui:$version
    ```

## Preview source

```
https://www.myget.org/F/netcorepal/api/v3/index.json
```

## Focus points

+ Beginner-friendly
    + Comprehensive documentation
    + Supporting tutorials
+ Modeling-friendly
    + Direct support for Domain-Driven Design modeling
+ Extension-friendly
    + Modular design
    + Support for module integration or replacement
    + Encourage customization and modification based on source code
+ Deployment-friendly
    + Support Docker
    + Support Helm
+ Testing-friendly
    + Emphasize support for unit testing and integration testing
+ AI-friendly
    + Continuously explore support for AI automated code generation with large language models

## Some principles

We do not reinvent the wheel, but rather organically organize excellent infrastructure to achieve our goals through good architectural conventions.

We continuously focus on collaboration efficiency. The architectural design of this project will continuously focus on the impact of the architecture on team collaboration and continuously improve.

We continuously focus on robustness and the quality of project code.
