# 代码分析源生成器

## 功能特性

- 自动分析项目中的领域驱动设计（DDD）相关代码关系
- 支持识别聚合、命令、命令处理器、领域事件、领域事件处理器、集成事件、集成事件处理器、集成事件转换器等核心元素
- 自动生成代码关系分析结果类，实现 `IAnalysisResult` 接口，便于后续图形化展示和分析
- 支持多项目代码关系分析结果合并
- 零侵入、无需手动标注，基于约定自动识别 DDD 结构
- 支持子实体与聚合、事件等复杂关系的自动归属与追踪
- 生成结果可用于架构可视化、链路追踪、文档生成等场景

---

## 分析结果

**分析并生成的元素：**
1. 发出命令的类型及其方法（如 Controller、事件处理器、以及其它任意类型等）
2. 命令
3. 聚合及其方法（包括静态方法、构造函数，子实体方法应被视作聚合的方法）
4. 领域事件及其处理器
5. 集成事件及其处理器
6. 集成事件转换器

**分析并生成的关系：**
1. 发出命令的方法 → 命令
2. 命令（即命令处理器）→ 聚合方法
3. 聚合方法 → 领域事件
4. 领域事件 → 集成事件转换器
5. 领域事件 → 领域事件处理器
6. 集成事件转换器 → 集成事件
7. 集成事件 → 集成事件处理器

> 注：事件处理器发出命令时，不展示处理器方法节点（方法固定）。

---

## 子实体处理规则

- 子实体作为聚合属性时，视为聚合的子实体，方法归属聚合（如 `OrderItem.UpdateQuantity` 视为 `Order` 的方法）。
- 子实体方法、事件分别归属到所有引用它的聚合。
- 子实体发出的领域事件，归属为聚合方法发出的事件。
- 命令处理器调用子实体方法时，视为调用聚合方法。
- 聚合及子实体方法内部调用自身其它方法发出事件时，记录方法与事件的关系。

---

## 源生成器规则

- 聚合：继承 `Entity<TId>`，`TId` 实现强类型ID接口（如 `IGuidStronglyTypedId`），并实现 `IAggregateRoot` 接口。
- 命令：实现 `ICommand` 或 `ICommand<TResponse>`
- 命令处理器：实现 `ICommandHandler<TCommand>` 或 `ICommandHandler<TCommand, TResponse>`
- 领域事件：实现 `IDomainEvent`
- 领域事件处理器：实现 `IDomainEventHandler<TDomainEvent>`
- 集成事件处理器：实现 `IIntegrationEventHandler<TIntegrationEvent>`
- 集成事件转换器：实现 `IIntegrationEventConverter<TDomainEvent, TIntegrationEvent>`

---

我期望将CodeFlowAnalysisSourceGenerator源生成器拆分成多个源生成器
1. 根据聚合代码生成聚合、聚合方法、聚合方法发出的事件
2. 根据命令代码生成命令与聚合方法之间的关系
3. 根据controller、endpoint 代码生成其方法与命令之间的关系
4. 根据任意方法的代码生成  命令发送者与命令之间的关系
5. 根据领域事件处理器 及其发出的命令 生成 领域事件处理器与命令之间的关系
6. 根据集成事件处理器 及其发出的命令 生成 集成事件处理器与命令之间的关系
7. 根据集成事件转换器 生成领域事件与集成事件之间的关系

源生成器不再生成IAnalysisResult的实现，而是分别生成 元数据，类似 Attribute 附加到程序集

## 使用说明

- 支持多项目代码关系分析结果合并。
- 生成的分析结果类实现 `IAnalysisResult`，供图形化展示使用。

---

## 编译与测试

- 编译前清理 `test/NetCorePal.Web/GeneratedFiles` 目录。
- 编译 `test/NetCorePal.Web` 项目，生成代码在 `GeneratedFiles` 目录。
- 运行测试可加 `--logger "console;verbosity=detailed"` 输出详细日志，便于调试。

```bash
dotnet test test/NetCorePal.Extensions.CodeAnalysis.UnitTests/NetCorePal.Extensions.CodeAnalysis.UnitTests.csproj --filter "FullyQualifiedName~GenerateMultiChainFlowChart_With_This_Assembly" --logger "console;verbosity=detailed"
```

---

## 测试用例覆盖说明

### 1. 聚合相关
- **聚合识别**
  - 能正确识别继承 Entity<TId> 且实现 IAggregateRoot 的类型为聚合。
  - 能识别 TId 是否实现强类型ID接口。
- **聚合方法识别**
  - 能识别聚合的实例方法、静态方法、构造函数。
  - 能识别子实体方法并归属到聚合。
- **聚合方法发出事件**
  - 能识别聚合方法内部发出的领域事件，并建立方法与事件的关系。
- **子实体归属**
  - 能识别子实体作为聚合属性时，方法归属聚合。
  - 能识别子实体方法、事件归属到所有引用它的聚合。
  - 能识别子实体发出的领域事件归属为聚合方法发出的事件。

### 2. 命令与聚合方法关系
- **命令识别**
  - 能识别实现 ICommand 或 ICommand<TResponse> 的类型为命令。
- **命令处理器识别**
  - 能识别实现 ICommandHandler<TCommand> 或 ICommandHandler<TCommand, TResponse> 的类型为命令处理器。
- **命令与聚合方法关系**
  - 能识别命令处理器调用聚合方法，并建立命令与聚合方法的关系。
  - 能识别命令处理器调用子实体方法时，视为调用聚合方法。

### 3. Controller/Endpoint 与命令关系
- **Controller/Endpoint 识别**
  - 能识别 Controller、Endpoint 中的方法。
- **方法与命令关系**
  - 能识别 Controller/Endpoint 方法发出的命令，并建立方法与命令的关系。

### 4. 任意方法的命令发送关系
- **命令发送者识别**
  - 能识别任意类型、任意方法中发出的命令。
- **命令发送关系**
  - 能建立命令发送者与命令之间的关系。

### 5. 领域事件处理器与命令关系
- **领域事件处理器识别**
  - 能识别实现 IDomainEventHandler<TDomainEvent> 的类型为领域事件处理器。
- **事件处理器发出命令**
  - 能识别领域事件处理器方法中发出的命令，并建立事件处理器与命令的关系。
  - 验证事件处理器发出命令时不展示处理器方法节点。

### 6. 集成事件处理器与命令关系
- **集成事件处理器识别**
  - 能识别实现 IIntegrationEventHandler<TIntegrationEvent> 的类型为集成事件处理器。
- **集成事件处理器发出命令**
  - 能识别集成事件处理器方法中发出的命令，并建立集成事件处理器与命令的关系。

### 7. 集成事件转换器关系
- **集成事件转换器识别**
  - 能识别实现 IIntegrationEventConverter<TDomainEvent, TIntegrationEvent> 的类型为集成事件转换器。
- **领域事件与集成事件关系**
  - 能建立领域事件与集成事件之间的关系。
  - 考虑一个领域事件可能对应多个集成事件，一个集成事件可能对应多个领域事件。

### 8. 元数据生成与附加
- **元数据生成**
  - 每类分析结果都能生成对应的元数据（而非 IAnalysisResult 实现类）。
- **元数据附加**
  - 生成的元数据能以 Attribute 形式附加到程序集。

### 9. 多项目合并
- **多项目分析结果合并**
  - 能正确合并多个项目的分析结果，保证关系完整、无重复。



## AggregateMethodEventMetadataGenerator

分析步骤：

1. 判断类型是否为聚合根，如果是则收集其所有方法，否则跳过
2. 递归收集聚合根所有属性、字段、集合，收集其所有子实体以及子实体的子实体
3. 递归聚合根以及其所有相关子实体，收集所有方法、方法与事件的关系，方法之间的调用关系
4. 根据方法与方法之间的关系，方法与事件的关系，生成聚合方法与事件的映射关系

子实体的方法视作聚合的方法，命名规则为 子实体.方法名
同名但参数签名不同的方法，视作同一个方法


## CommandToAggregateMethodMetadataGenerator
分析步骤：

1. 判断类型是否为命令，并记录
2. 判断类型是否为命令处理器，如果是则收集其方法与聚合方法之间的关系
3. 生成命令与聚合方法之间的关系元数据，如果没有关系则聚合方法留空

命令可以是Record类型，也可以是class类型


## DomainEventToHandlerMetadataGenerator
分析步骤：

1. 判断类型是否为领域事件，如果是则收集
2. 判断类型是否为领域事件处理器，如果是则收集其方法与领域事件之间的关系
3. 生成领域事件与处理器之间的关系元数据，如果没有关系则处理器留空

命令可以是Record类型，也可以是class类型