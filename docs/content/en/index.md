# netcorepal-cloud-framework

[![Release Build](https://img.shields.io/github/actions/workflow/status/netcorepal/netcorepal-cloud-framework/release.yml?label=release%20build)](https://github.com/netcorepal/netcorepal-cloud-framework/actions/workflows/release.yml)
[![Preview Build](https://img.shields.io/github/actions/workflow/status/netcorepal/netcorepal-cloud-framework/dotnet.yml?label=preview%20build)](https://github.com/netcorepal/netcorepal-cloud-framework/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/NetCorePal.Extensions.AspNetCore.svg)](https://www.nuget.org/packages/NetCorePal.Extensions.AspNetCore)
[![MyGet Preview](https://img.shields.io/myget/netcorepal/vpre/NetCorePal.Extensions.AspNetCore?label=preview)](https://www.myget.org/feed/netcorepal/package/nuget/NetCorePal.Extensions.AspNetCore)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/netcorepal/netcorepal-cloud-framework/blob/main/LICENSE)

A `tactical` framework for `Domain-Driven Design` based on `ASP.NET Core`.

## Vision

With the development of the .NET technology ecosystem, its microservice architecture in the cloud-native era has become very mature, and the implementation of domain-driven design has also received very good support. At the same time, with the increasing demand for informatization and digitalization in various industries, more enterprises and teams are beginning to pay attention to how to effectively organize R&D teams and realize their own business architecture.

The core purpose of this project is to help enterprises quickly build a technical implementation framework based on domain-driven design, and to accumulate and explore domain-driven design methodologies, so that more enterprises and teams can benefit.

## Focus Areas

+ Beginner-friendly
  + Comprehensive documentation
  + Supporting tutorials
+ Modeling-friendly
  + Direct support for domain-driven design modeling
+ Extension-friendly
  + Modular design
  + Support for module integration or replacement
  + Encourage customization and modification based on source code
+ Deployment-friendly
  + Support for Docker
  + Support for Helm
+ Testing-friendly
  + Emphasis on support for unit testing and integration testing
+ AI-friendly
  + Continuous exploration of support for AI automated code generation with large language models

## Principles

We do not reinvent the wheel, but organically organize excellent infrastructure to achieve our goals through good architectural conventions.

We continuously focus on collaboration efficiency. The architectural design of this project will continuously pay attention to the impact of the architecture on team collaboration and continuously improve.

We continuously focus on robustness and the quality of project code.

## How to Use

Use the `NetCorePal.Template` template tool to create a project:

```cmd
# Install the template tool
dotnet new -i NetCorePal.Template
# Create a project
dotnet new netcorepal-web -n My.Project.Name
```

Template tool: <https://github.com/netcorepal/netcorepal-cloud-template>

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
  + Based on .NET Aspire

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

For more information, please refer to the [documentation](docs/index.md).

## Referenced Projects

+ [AspNetCore](https://github.com/dotnet/aspnetcore)
+ [EntityFrameworkCore](https://github.com/dotnet/efcore)
+ [MediatR](https://github.com/jbogard/MediatR)
+ [DotNetCore.CAP](https://github.com/dotnetcore/CAP)
+ [KubernetesClient](https://github.com/kubernetes-client/csharp)
+ [DistributedLock.Redis](https://github.com/madelson/DistributedLock)

## Collaboration

We have an open mindset and welcome anyone to provide opinions and suggestions, and also welcome anyone to contribute code.

## Development and Debugging

1. Install `.NET 8.0 SDK` or higher.

    SDK download: <https://dot.net/download>

2. Have a `Docker` environment for automated unit testing and integration testing.

    `Docker Desktop` download: <https://www.docker.com/products/docker-desktop/>

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

## Preview Source

```
https://www.myget.org/F/netcorepal/api/v3/index.json
```
