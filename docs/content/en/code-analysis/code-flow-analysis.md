# Code Flow Analysis

## Overview

`NetCorePal.Extensions.CodeAnalysis` automatically analyzes your code structure via source generators, helping you understand the relationships and data flow between components in DDD architecture, and supports multiple visualization methods.

## Features

- Automatically identifies command senders, aggregate roots, commands, events, handlers, and other types
- Automatically establishes relationships between methods, commands, aggregates, events, handlers, etc.
- Supports automatic generation of various Mermaid diagrams
- One-click generation of interactive HTML architecture visualization pages

## Usage

### 1. Install the Package

Add the following to the project you want to analyze:

```xml
<PackageReference Include="NetCorePal.Extensions.CodeAnalysis" />
```

### 2. Enable Source Generator

Simply reference the `NetCorePal.Extensions.CodeAnalysis` package; no manual configuration is required.

The built-in SourceGenerator will automatically scan your project code at compile time, analyze controllers, commands, aggregate roots, events, handlers, and their relationships, and automatically generate a data file containing the analysis results.

This file contains all the analysis result data structures for runtime aggregation and visualization.

The entire process is fully automated, with no extra steps required, and supports multi-project and cross-assembly analysis.

### 3. Obtain Analysis Results

```csharp
using NetCorePal.Extensions.CodeAnalysis;
var result = CodeFlowAnalysisHelper.GetResultFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
```

// It is recommended to aggregate all assemblies in the current application domain in ASP.NET Core projects to ensure complete analysis results.

## Mermaid Visualization Support

The framework provides three built-in Mermaid Visualizers:

1. **ArchitectureOverviewMermaidVisualizer**  
   Generates a complete architecture diagram of all types and their relationships in the system.

   ```csharp
   var mermaid = ArchitectureOverviewMermaidVisualizer.GenerateMermaid(result);
   ```

2. **ProcessingFlowMermaidVisualizer (Processing Flow Diagram)**  
   Generates a collection of flow diagrams for all independent business processing chains (one diagram per chain), showing the actual invocation relationships of commands, events, aggregates, etc. in business flows.

   ```csharp
   var chains = ProcessingFlowMermaidVisualizer.GenerateMermaid(result);
   foreach (var (name, diagram) in chains)
   {
       Console.WriteLine($"{name}:\n{diagram}");
   }
   ```

3. **AggregateRelationMermaidVisualizer**  
   Generates a collection of relationship diagrams for all aggregate roots (one diagram per aggregate root).

   ```csharp
   var aggregates = AggregateRelationMermaidVisualizer.GenerateAllAggregateMermaid(result);
   foreach (var (aggName, diagram) in aggregates)
   {
       Console.WriteLine($"{aggName}:\n{diagram}");
   }
   ```

## Interactive HTML Visualization

Use `VisualizationHtmlBuilder` to generate a complete interactive HTML architecture page with all diagrams and navigation in one click:

```csharp
var html = VisualizationHtmlBuilder.GenerateVisualizationHtml(result, "My Architecture Visualization");
File.WriteAllText("architecture-visualization.html", html);
```

- Supports sidebar navigation, diagram switching, and one-click jump to Mermaid Live Editor
- Includes all chains, aggregates, architecture overview diagrams, etc.

## ASP.NET Core Middleware Integration

Integrate the online architecture analysis diagram in development environment with just one line of code:

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

## Online Preview of Mermaid Diagrams

All Mermaid code can be pasted into [Mermaid Live Editor](https://mermaid.live/edit) for real-time preview and editing. The HTML page includes a one-click jump button.