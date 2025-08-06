# 代码流分析 (Code Flow Analysis)

## 概述

`NetCorePal.Extensions.CodeAnalysis` 通过源生成器自动分析您的代码结构，帮助理解 DDD 架构中各组件关系和数据流向，并支持多种可视化方式。

## 功能特性

- 自动识别命令发送者、聚合根、命令、事件、处理器等类型
- 自动建立方法、命令、聚合、事件、处理器等多种关系
- 支持多种 Mermaid 图表自动生成
- 一键生成交互式 HTML 架构可视化页面

## 使用方法

### 1. 安装包

在需要分析的项目中添加：

```xml
<PackageReference Include="NetCorePal.Extensions.CodeAnalysis" />
```


### 2. 启用源生成器

只需引用 `NetCorePal.Extensions.CodeAnalysis` 包，无需手动配置。


其内置的 SourceGenerator 会在编译时自动扫描项目代码，分析控制器、命令、聚合根、事件、处理器等类型及其关系，并自动生成包含分析结果的数据文件。

该文件包含所有分析结果数据结构，供运行时聚合和可视化使用。

整个流程完全自动化，无需额外步骤，支持多项目、跨程序集分析。


### 3. 获取分析结果

```csharp
using NetCorePal.Extensions.CodeAnalysis;
var result = CodeFlowAnalysisHelper.GetResultFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
```

// 推荐在 ASP.NET Core 项目中直接聚合当前应用域所有程序集，确保分析结果完整。

## Mermaid 可视化支持

框架内置三种 Mermaid Visualizer：

1. **ArchitectureOverviewMermaidVisualizer**  
   生成系统所有类型及其关系的完整架构图。

   ```csharp
   var mermaid = ArchitectureOverviewMermaidVisualizer.GenerateMermaid(result);
   ```

2. **ProcessingFlowMermaidVisualizer（处理流程图）**  
   生成所有独立业务处理链路的流程图集合（每个独立流程一张图），用于展示命令、事件、聚合等在业务流转中的实际调用关系。

   ```csharp
   var chains = ProcessingFlowMermaidVisualizer.GenerateMermaid(result);
   foreach (var (name, diagram) in chains)
   {
       Console.WriteLine($"{name}:\n{diagram}");
   }
   ```

3. **AggregateRelationMermaidVisualizer**  
   生成所有聚合根的关系图集合（每个聚合根一张图）。

   ```csharp
   var aggregates = AggregateRelationMermaidVisualizer.GenerateAllAggregateMermaid(result);
   foreach (var (aggName, diagram) in aggregates)
   {
       Console.WriteLine($"{aggName}:\n{diagram}");
   }
   ```

## 交互式 HTML 可视化

通过 `VisualizationHtmlBuilder` 一键生成完整的交互式 HTML 架构页面，包含所有图表和导航：

```csharp
var html = VisualizationHtmlBuilder.GenerateVisualizationHtml(result, "我的架构可视化");
File.WriteAllText("architecture-visualization.html", html);
```

- 支持侧边栏导航、图表切换、Mermaid Live Editor 一键跳转
- 包含所有链路、聚合、架构总览等图表

## ASP.NET Core 中间件集成

一行代码即可在开发环境下集成在线架构分析图：

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapGet("/diagnostics/code-analysis", () =>
        VisualizationHtmlBuilder.GenerateVisualizationHtml(
            AnalysisResultAggregator.Aggregate(new[] { Assembly.GetExecutingAssembly() })
        )
    );
}
```

## Mermaid 图表在线预览

所有 Mermaid 代码均可粘贴到 [Mermaid Live Editor](https://mermaid.live/edit) 实时预览和编辑。HTML 页面内置一键跳转按钮。