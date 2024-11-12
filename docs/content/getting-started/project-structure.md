# 项目结构

## 整体结构
```text
YourProject
     ├──src
     │  ├──YourProject.Web  //Web项目
     │  ├──YourProject.Domain //领域模型
     │  └──YourProject.Infrastructure //基础设施
     └──tests
        ├──YourProject.Web.Tests  //Web测试
        ├──YourProject.Domain.Tests //领域模型测试
        └──YourProject.Infrastructure.Tests //基础设施测试
```

## 领域模型层项目结构
    
```text
YourProject.Domain
     ├── AggregatesModel //领域模型目录，放置聚合根、实体、值对象等
     └── DomainEvents //领域事件目录
```

## 基础设施层项目结构

```text
YourProject.Infrastructure
     ├── EntityConfigurations  //领域模型数据库映射配置目录
     ├── Repositories  //仓储目录
     └── ApplicationDbContext.cs  //数据库上下文
```

## Web层项目结构

```text
YourProject.Web
     ├── wwwroot //静态资源目录
     ├── Application  //应用服务目录
     │   ├── Commands  //命令、命令处理器、命令验证器目录
     │   ├── DomainEventHandlers  //领域事件处理器目录
     │   ├── IntegrationEventConverters //集成事件转换器目录
     │   ├── IntegrationEventHandlers  //集成事件处理器目录
     │   └── Queries  //查询服务目录
     ├── Clients  //远程服务客户端目录，用以访问其它微服务或第三方服务
     ├── Controllers  //控制器目录
     ├── Extensions  //扩展方法目录，放置各种扩展方法定义
     ├── Filters  //过滤器目录
     └── Program.cs  //启动入口类
```