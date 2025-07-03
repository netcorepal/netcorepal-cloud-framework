# 代码关系分析源生成器

## 功能特性

需求时在图中展示下面元素：

1. 发出命令的类型
2. 发出命令的类型的方法
3. 聚合
4. 聚合的方法
5. 聚合方法发出的领域事件
6. 集成事件
7. 领域事件处理器
8. 集成事件处理器

需要展示下面关系：

1. 发出命令的方法与命令的关系
2. 命令对应的处理器与其调用的聚合方法的关系（注意，命令处理器与命令一一对应，因此命令即可代表命令处理器）
3. 聚合方法与其发出的领域事件的关系
4. 领域事件被集成事件转换器（集成事件转换器是实现接口IIntegrationEventConverter的类型）转换对应的集成事件的关系
5. 领域事件与领域事件处理器的关系
6. 集成事件与集成事件处理器的关系

如果是领域事件处理器、集成事件处理器发出命令，则不需要展示处理器方法，因为我们定义了事件处理器固定的处理方法

不需要记录和展示上述元素和关系之外的内容


## 源生成器规则


聚合必须继承基类Entity<TId> ，Tid必须实现强类ID接口，例如IGuidStronglyTypedId
命令必须继承ICommand或者ICommand<out TResponse>
命令处理器必须实现接口ICommandHandler<in TCommand>或者ICommandHandler<in TCommand, TResponse> 
领域事件必须继承IDomainEvent接口
事件处理器必须实现IDomainEventHandler<in TDomainEvent> 接口
集成事件处理器必须实现IIntegrationEventHandler<in TIntegrationEvent> 接口
集成事件转换器必须实现IIntegrationEventConverter<in TDomainEvent, out TIntegrationEvent>接口


## 使用场景

一般情况，我们的代码会分布在 Domain、Infrastructure、Web 等多个项目中，需要支持将多个项目的代码关系分析结果合并到一起。

## 生成结果

生成代码共享的部分，已经定义在项目NetCorePal.Extensions.CodeAnalysis中了。
生成一个实现IAnalysisResult接口的类，存储并返回代码关系分析结果，以为后续生成图形化的代码关系分析结果提供数据支持。

## 编译验证

编译 test/NetCorePal.Web 项目来验证，代码会生成在目录test/NetCorePal.Web/GeneratedFiles 目录下
编译前，需要清理test/NetCorePal.Web/GeneratedFiles 目录下的所有文件
