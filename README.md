# netcorepal-cloud-framework

[![Release Build](https://img.shields.io/github/actions/workflow/status/netcorepal/netcorepal-cloud-framework/release.yml?label=release%20build)](https://github.com/netcorepal/netcorepal-cloud-framework/actions/workflows/release.yml)
[![Preview Build](https://img.shields.io/github/actions/workflow/status/netcorepal/netcorepal-cloud-framework/dotnet.yml?label=preview%20build)](https://github.com/netcorepal/netcorepal-cloud-framework/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/NetCorePal.Extensions.AspNetCore.svg)](https://www.nuget.org/packages/NetCorePal.Extensions.AspNetCore)
[![MyGet Preview](https://img.shields.io/myget/netcorepal/vpre/NetCorePal.Extensions.AspNetCore?label=preview)](https://www.myget.org/feed/netcorepal/package/nuget/NetCorePal.Extensions.AspNetCore)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/netcorepal/netcorepal-cloud-framework/blob/main/LICENSE)

一个基于`ASP.NET Core`实现的`领域驱动设计`落地`战术`框架。

A `tactical` framework for `Domain-Driven Design` based on `ASP.NET Core`.

核心特性：
+ 领域驱动设计实践支持
+ CQRS
+ Event Driven
+ 分布式事务（事件处理的最终一致性）
+ 多租户
+ 多环境（灰度发布）

## 文档

<https://netcorepal.github.io/netcorepal-cloud-framework>

## 愿景

随着 .NET
技术生态的发展，其在云原生时代的微服务架构已经发展得非常成熟，而领域驱动设计的落地也得到了非常好的支持。同时随着各行各业的信息化、数字化发展诉求越发强烈，更多的企业和团队也对如何有效地组织研发团队以及实现自己的业务架构这个课题开始投入关注。

本项目的核心目的是帮助企业快速构建一套基于领域驱动设计的技术实现框架，同时在领域驱动设计方法论方面进行沉淀和探讨，从而让更多的企业和团队得到帮助。

## 关注重点

+ 入门友好
  + 完善的文档
  + 配套教程
+ 建模友好
  + 对领域驱动设计建模的直接支持
+ 扩展友好
  + 模块化设计
  + 支持模块集成或替换
  + 鼓励基于源码定制修改
+ 部署友好
  + 支持Docker
  + 支持Helm
+ 测试友好
  + 强调对单元测试、集成测试的支持
+ AI 友好
  + 持续探索对大语言模型AI自动化代码生成的支持

## 一些原则

我们不重复造轮子，更多地是有机地将优秀的基础设施组织起来，通过建立良好的架构约定来达到目的。

我们持续关注协作效率，本项目的架构设计，会持续关注架构对团队协作的影响，并持续改进。

我们持续关注健壮性，持续关注项目代码的质量。

## 如何使用

使用 `NetCorePal.Template` 模板工具创建项目:

```cmd
# 安装模板工具
dotnet new -i NetCorePal.Template
# 创建项目
dotnet new netcorepal-web -n My.Project.Name
```

模板工具：<https://github.com/netcorepal/netcorepal-cloud-template>

## Roadmap

规划提供的能力

+ [x] 支持灵活配置与部署的网关
+ [x] 基于 `ASP.NET Core`和开源组件的快速开发框架
+ [x] 提供领域驱动设计实现的代码模板工程脚手架
+ [x] 实现具备业务扩展性的整体灰度解决方案
+ [x] 实现具备业务扩展性的租户能力
+ [x] 基于领域驱动设计的微服务架构实践
+ [x] 模块化的设计，可按需使用、按需替换
+ [ ] 提供详实的文档
+ [ ] 提供带有可视化操作界面的微服务基础设施

## 组件说明

+ [x] Context Passing
  + [x] AspNetCore (HTTP Request)
  + [x] HttpClient
  + [x] RabbitMQ (Based on DotNetCore.CAP)
+ [x] Domain
  + [x] Entity
  + [x] StronglyTypedId (With Source Generator)
  + [x] ValueObject
  + [x] AggregateRoot
  + [x] DomainEvent
+ [x] Repository (Based On EntityFrameworkCore)
+ [x] Transaction
  + [x] UnitOfWork
  + [x] Distributed Transaction
    + [x] Outbox(Based on DotNetCore.CAP)
+ [x] IdGeneration
  + [x] Snowflake
    + [x] Snowflake with Etcd
    + [x] Snowflake with Redis
    + [x] Snowflake with Consul
+ [x] Mappers
+ [x] Primitives
  + [x] Exception Handling
  + [x] Clock
+ [x] Service Discovery
  + [x] Service Discovery Kubernetes
  + [ ] Service Discovery Consul
  + [ ] Service Discovery Nacos
+ [x] Multi Tenant
+ [x] Multi Environment
  + [x] Gray Environment

更多信息请参阅[使用文档](docs/index.md)

## 引用项目

+ [AspNetCore](https://github.com/dotnet/aspnetcore)
+ [EntityFrameworkCore](https://github.com/dotnet/efcore)
+ [MediatR](https://github.com/jbogard/MediatR)
+ [DotNetCore.CAP](https://github.com/dotnetcore/CAP)
+ [KubernetesClient](https://github.com/kubernetes-client/csharp)
+ [DistributedLock.Redis](https://github.com/madelson/DistributedLock)

## 关于协作

我们具有开放的心态，欢迎任何人提出意见和建议，也欢迎任何人贡献代码。

## 开发调试

1. 安装`.NET 9.0 SDK`或更高版本。

    SDK下载地址： <https://dot.net/download>

2. 拥有`Docker`环境，用于自动化单元测试和集成测试。

    `Docker Desktop`下载地址： <https://www.docker.com/products/docker-desktop/>

3. 构建项目

    ```shell
    dotnet build
    ```

4. 运行测试

    ```shell
    dotnet test
    ```

5. 其它可选依赖

    安装`skywalking`

    ```shell
    # 安装oap
    docker run --name oap -p 11800:11800 -p 12800:12800 --restart always -d apache/skywalking-oap-server:9.0.0

    # 安装oap-ui
    export version=9.0.0
    docker run --name oap-ui -p 8080:8080 -d --link oap -e SW_OAP_ADDRESS=http://oap:12800  apache/skywalking-ui:$version

    ```


## 预览版源

```
https://www.myget.org/F/netcorepal/api/v3/index.json
```
