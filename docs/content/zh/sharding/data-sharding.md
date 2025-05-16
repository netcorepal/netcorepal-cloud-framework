# 分库分表解决方案

为了使系统具备更大规模的数据容量扩展能力，我们集成了 [sharding-core](https://github.com/dotnetcore/sharding-core) 作为数据分库分表的解决方案。

`sharding-core` 是一个基于 `EntityFrameworkCore` 的开源的分库分表框架，支持多种数据库类型和分片策略。作为客户端解决方案，它对于数据库基础设施的要求较低，适合于大多数场景。它支持多种分片策略，包括范围分片、哈希分片和复合分片等。它还支持动态分片和动态路由，可以根据业务需求灵活调整。

## 支持的场景

+ [读写分离](read-write-separation.md)
+ [分表](sharding-table.md)
+ [分库](sharding-database.md)
+ [租户模式](sharding-tenant.md)

`读写分离`、`分表`、`分库`可以按需组合搭配使用。

## 官方文档

关于 `sharding-core` 的更多信息，可以参考官方文档： https://xuejmnet.github.io/sharding-core-doc/

## 注意事项

### 关于懒加载兼容

需要添加 o.UseEntityFrameworkCoreProxies = true;

see: https://github.com/dotnetcore/sharding-core/issues/259