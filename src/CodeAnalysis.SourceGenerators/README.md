# 代码流分析源生成器

## 概述

这个源生成器可以分析您的 .NET 项目，自动识别和分析以下代码元素之间的调用关系：

- **Controller** → **Command** 
- **Command** → **Entity** 方法
- **Entity** 方法 → **DomainEvent**
- **DomainEvent** → **DomainEventHandler**
- **DomainEventHandler** → **Command**

## 功能特性

### 1. 代码元素识别
- 自动识别 Controller、Command、CommandHandler、Entity、DomainEvent、DomainEventHandler
- 支持聚合根（IAggregateRoot）识别
- 支持 Record 类型的 Command 和 DomainEvent

### 2. 调用关系分析
- Controller 中通过 `mediator.Send()` 调用的 Command
- Entity 方法中通过 `AddDomainEvent()` 创建的 DomainEvent
- DomainEventHandler 中发出的 Command
- CommandHandler 中调用的 Entity 方法

### 3. 多种输出格式
- C# 静态类：便于在代码中查询
- JSON 格式：便于外部工具集成
- Mermaid 图表：便于可视化
- 统计信息：提供项目概览

## 安装和使用

### 1. 添加项目引用

在您的项目文件中添加：

```xml
<ItemGroup>
  <ProjectReference Include="path\to\NetCorePal.Extensions.CodeAnalysis.SourceGenerators.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### 2. 构建项目

构建项目后，源生成器会自动生成以下文件：

- `CodeFlowAnalysis.g.cs` - 主要的分析结果类
- `CodeFlowAnalysis.json.g.cs` - JSON 格式的分析数据
- `CodeFlowMermaid.g.cs` - Mermaid 图表代码
- `CodeFlowStatistics.g.cs` - 统计信息

## 使用示例

### 1. 查询 Controller 调用的 Command

```csharp
// 获取 OrderController 调用的所有命令
var commands = EnhancedCodeFlowAnalysis.GetCommandsCalledByController("OrderController");
foreach (var command in commands)
{
    Console.WriteLine($"OrderController calls: {command}");
}
```

### 2. 查询 Entity 方法创建的 DomainEvent

```csharp
// 获取 Order 实体的 Create 方法创建的领域事件
var domainEvents = EnhancedCodeFlowAnalysis.GetDomainEventsCreatedByEntityMethod("Order", "Create");
foreach (var domainEvent in domainEvents)
{
    Console.WriteLine($"Order.Create creates: {domainEvent}");
}
```

### 3. 查询 DomainEventHandler 发出的 Command

```csharp
// 获取 OrderCreatedDomainEventHandler 发出的命令
var issuedCommands = EnhancedCodeFlowAnalysis.GetCommandsIssuedByDomainEventHandler("OrderCreatedDomainEventHandler");
foreach (var command in issuedCommands)
{
    Console.WriteLine($"OrderCreatedDomainEventHandler issues: {command}");
}
```

### 4. 获取统计信息

```csharp
// 显示项目统计信息
var summary = CodeFlowStatistics.GetSummary();
Console.WriteLine(summary);

// 输出示例：
// Code Flow Analysis Summary:
// Controllers: 5
// Commands: 12
// Command Handlers: 12
// Entities: 8
// Aggregate Roots: 3
// Domain Events: 6
// Domain Event Handlers: 6
// Total Relationships: 25
```

### 5. 生成 Mermaid 图表

```csharp
// 获取 Mermaid 流程图代码
var mermaidCode = CodeFlowMermaidDiagram.FlowChart;
Console.WriteLine(mermaidCode);

// 可以将此代码复制到支持 Mermaid 的工具中进行可视化
```

### 6. 获取 JSON 数据

```csharp
// 获取完整的分析数据（JSON 格式）
var jsonData = CodeFlowAnalysisJson.Data;
// 可以将此数据导出到外部工具进行进一步分析
```

## 支持的模式识别

### Controller 识别模式
- 类名以 "Controller" 结尾
- 继承自 `ControllerBase` 或 `Controller`
- 实现 `IController` 接口

### Command 识别模式
- 类名以 "Command" 结尾
- 实现 `ICommand` 或 `ICommand<T>` 接口
- 实现 `IBaseCommand` 接口

### Entity 识别模式
- 继承自 `Entity` 或 `Entity<T>` 基类
- 支持聚合根（`IAggregateRoot`）识别

### DomainEvent 识别模式
- 类名以 "DomainEvent" 结尾
- 实现 `IDomainEvent` 接口
- 支持 Record 类型

### DomainEventHandler 识别模式
- 类名以 "DomainEventHandler" 结尾
- 实现 `IDomainEventHandler<T>` 接口

## 调用关系检测

### 1. mediator.Send() 调用检测
```csharp
// 在 Controller 或 DomainEventHandler 中
await mediator.Send(new CreateOrderCommand(...));
```

### 2. AddDomainEvent() 调用检测
```csharp
// 在 Entity 方法中
this.AddDomainEvent(new OrderCreatedDomainEvent(this));
```

### 3. Entity 方法调用检测
```csharp
// 在 CommandHandler 中
order.UpdateStatus(status);
```

## 输出示例

### 生成的 C# 代码示例

```csharp
public static class EnhancedCodeFlowAnalysis
{
    public static readonly List<ControllerAnalysis> Controllers = new()
    {
        // OrderController 的分析结果
    };

    public static readonly List<CommandAnalysis> Commands = new()
    {
        // CreateOrderCommand 的分析结果
    };

    // ... 其他集合

    public static List<string> GetCommandsCalledByController(string controllerName)
    {
        // 实现查询逻辑
    }
}
```

### JSON 输出示例

```json
{
  "Controllers": [
    {
      "Type": {
        "Name": "OrderController",
        "FullName": "MyApp.Controllers.OrderController",
        "Namespace": "MyApp.Controllers"
      },
      "Actions": [
        {
          "Name": "CreateOrder",
          "HttpMethod": "POST",
          "RouteTemplate": "/api/orders",
          "Commands": ["MyApp.Commands.CreateOrderCommand"]
        }
      ]
    }
  ],
  "Relationships": [
    {
      "Source": {
        "TypeName": "MyApp.Controllers.OrderController",
        "MethodName": "CreateOrder"
      },
      "Target": {
        "TypeName": "MyApp.Commands.CreateOrderCommand",
        "MethodName": "Handle"
      },
      "CallType": "ControllerToCommand"
    }
  ]
}
```

## 扩展和自定义

如果您需要自定义识别模式或添加新的分析类型，可以：

1. 修改 `EnhancedCodeFlowAnalyzer` 类中的识别方法
2. 添加新的数据模型类
3. 扩展调用关系检测逻辑

## 注意事项

1. 源生成器在编译时运行，确保您的项目能够正常编译
2. 生成的代码文件会自动更新，不要手动修改
3. 复杂的动态调用可能无法被检测到
4. 确保您的代码遵循框架的约定模式以获得最佳分析效果

## 故障排除

### 生成的文件为空
- 检查项目是否正确引用了源生成器
- 确认项目中存在可识别的代码模式

### 某些调用关系未被检测到
- 检查是否使用了标准的调用模式（如 `mediator.Send()`）
- 确认类型命名遵循约定

### 编译错误
- 检查生成的代码是否有语法错误
- 确认所有必要的 using 语句都已包含

## 许可证

本项目遵循 MIT 许可证。
