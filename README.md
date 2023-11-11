# 愿景

随着 .NET
技术生态的发展，其在云原生时代的微服务架构已经发展得非常成熟，而领域驱动设计的落地也得到了非常好的支持。同时随着各行各业的信息化、数字化发展诉求越发强烈，更多的企业和团队也对如何有效地组织研发团队以及实现自己的业务架构这个课题开始投入关注。

本项目的核心目的是帮助企业快速构建一套基于领域驱动设计的技术实现框架，同时在领域驱动设计方法论方面进行沉淀和探讨，从而让更多的企业和团队得到帮助。

# 一些原则

我们不重复造轮子，更多地是有机地将优秀的基础设施组织起来，通过建立良好的架构约定来达到目的。

我们持续关注协作效率，本项目的架构设计，会持续关注架构对团队协作的影响，并持续改进。

我们持续关注健壮性，持续关注项目代码的质量。

# Roadmap

规划提供的能力

+ [ ] 支持灵活配置与部署的网关
+ [ ] 基于 `ASP.NET Core`和开源组件的快速开发框架
+ [ ] 提供领域驱动设计实现的代码模板工程脚手架
+ [ ] 实现具备业务扩展性的整体灰度解决方案
+ [ ] 实现具备业务扩展性的租户能力
+ [ ] 提供带有可视化操作界面的微服务基础设施
+ [ ] 模块化的设计，可按需使用、按需替换
+ [ ] 提供详实的文档
+ [ ] 基于领域驱动设计的微服务架构实践

# 组件说明

+ [x] Context
    + [x] AspNetCore (HTTP Request)
+ [ ] Domain
    + [x] Entity
    + [x] StronglyTypedId (With Source Generator
    + [x] ValueObject
    + [x] AggregateRoot
    + [x] DomainEvent
+ [x] Repository
+ [ ] Transaction
    + [x] UnitOfWork
    + [x] Distributed Transaction(With Source Generator
        + [x] Outbox
        + [ ] Saga
+ [x] IdGeneration
    + [x] Snowflake
        + [x] Snowflake with Etcd
+ [x] Mappers
+ [x] Primitives
    + [x] Exception Handling
    + [x] Clock
+ [x] Service Discovery
    + [x] Service Discovery Kubernetes
    + [ ] Service Discovery Eureka
    + [ ] Service Discovery Consul
    + [ ] Service Discovery Nacos
+ [ ] Multi Tenant
+ [x] Multi Environment

更多信息请参阅[使用文档](docs/index.md)
# 引用项目

+ [AspNetCore](https://github.com/dotnet/aspnetcore)
+ [EntityFrameworkCore](https://github.com/dotnet/efcore)
+ [MediatR](https://github.com/jbogard/MediatR)
+ [DotNetCore.CAP](https://github.com/dotnetcore/CAP)
+ [KubernetesClient](https://github.com/kubernetes-client/csharp)
+ [DistributedLock.Redis](https://github.com/madelson/DistributedLock)

# 关于协作

我们具有开放的心态，欢迎任何人提出意见和建议，也欢迎任何人贡献代码。