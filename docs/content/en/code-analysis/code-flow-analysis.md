# Code Flow Analysis

## Overview

`NetCorePal.Extensions.CodeAnalysis` provides powerful code flow analysis capabilities through the `CodeFlowAnalysisSourceGenerator` source generator, which automatically analyzes your code structure to help you understand the relationships and data flow between components in DDD architecture.

## Features

### 🔍 Automatic Code Analysis

- **Command Sender Detection**: Automatically identifies controllers, endpoints, event handlers, and other types that send commands
- **Aggregate Root Recognition**: Detects aggregate roots that implement the `IAggregateRoot` interface
- **Command Recognition**: Identifies command types that implement the `ICommand` interface
- **Event Detection**: Automatically discovers domain events and integration events
- **Handler Analysis**: Identifies various event handlers and converters

### 🔄 Relationship Mapping

The source generator automatically establishes the following relationships:

1. **Method to Command**: Relationships between methods that send commands and their corresponding commands
2. **Command to Aggregate Method**: Relationships between command handlers and aggregate methods they invoke
3. **Method to Domain Event**: Relationships between aggregate methods and domain events they emit
4. **Domain Event to Integration Event**: Conversion relationships through converters
5. **Domain Event to Handler**: Relationships between domain events and their handlers
6. **Integration Event to Handler**: Relationships between integration events and their handlers

## Usage

### 1. Install Packages

Add the following package reference to all projects that need to be analyzed:

```xml
<PackageReference Include="NetCorePal.Extensions.CodeAnalysis" />
```

> **Note**: The `NetCorePal.Extensions.CodeAnalysis` package already includes the source generator, so there's no need to install the source generator package separately. Make sure to add this package reference to all projects that need to be analyzed.

### 2. Enable Source Generator

The source generator runs automatically at compile time without additional configuration. After compilation, it generates the `CodeFlowAnalysisResult.g.cs` file.

### 3. Access Analysis Results

```csharp
using System.Reflection;
using NetCorePal.Extensions.CodeAnalysis;

// Use AnalysisResultAggregator static class to get analysis results
// Need to pass assemblies containing the code to be analyzed
var assemblies = new[] { Assembly.GetExecutingAssembly() }; // Or other assemblies to be analyzed
var result = AnalysisResultAggregator.Aggregate(assemblies);

// Access various component information
var controllers = result.Controllers;
var commands = result.Commands;
var entities = result.Entities;
var domainEvents = result.DomainEvents;
var relationships = result.Relationships;
```

> **Note**: The `Aggregate` method requires one or more assemblies as parameters that contain the code to be analyzed. You can pass the current assembly, specific business assemblies, or multiple assemblies from your project.

## Supported Code Patterns

### Controllers and Endpoints

```csharp
// ASP.NET Core Controller
[ApiController]
public class UserController : ControllerBase
{
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        await _mediator.Send(command);
        return Ok();
    }
}

// FastEndpoints Endpoint
public class CreateUserEndpoint : Endpoint<CreateUserCommand>
{
    public override async Task HandleAsync(CreateUserCommand command, CancellationToken ct)
    {
        await SendAsync(command, ct);
    }
}
```

### Commands and Handlers

```csharp
// Command
public record CreateUserCommand(string Name, string Email) : ICommand;

// Command Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand>
{
    public async Task<Unit> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User(request.Name, request.Email);
        // Business logic...
        return Unit.Value;
    }
}
```

### Aggregate Roots and Domain Events

```csharp
// Aggregate Root
public class User : IAggregateRoot
{
    public User(string name, string email)
    {
        Name = name;
        Email = email;
        AddDomainEvent(new UserCreatedDomainEvent(Id, name, email));
    }
    
    public void UpdateProfile(string name, string email)
    {
        Name = name;
        Email = email;
        AddDomainEvent(new UserUpdatedDomainEvent(Id, name, email));
    }
}

// Domain Event
public record UserCreatedDomainEvent(Guid UserId, string Name, string Email) : IDomainEvent;
```

### Event Handlers and Converters

```csharp
// Domain Event Handler
public class UserCreatedDomainEventHandler : IDomainEventHandler<UserCreatedDomainEvent>
{
    public async Task HandleAsync(UserCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Handle domain event
        await _mediator.Send(new SendWelcomeEmailCommand(domainEvent.UserId));
    }
}

// Integration Event Converter
public class UserCreatedIntegrationEventConverter : IIntegrationEventConverter<UserCreatedDomainEvent, UserCreatedIntegrationEvent>
{
    public UserCreatedIntegrationEvent Convert(UserCreatedDomainEvent domainEvent)
    {
        return new UserCreatedIntegrationEvent(domainEvent.UserId, domainEvent.Name, domainEvent.Email);
    }
}

// Integration Event Handler
public class UserCreatedIntegrationEventHandler : IIntegrationEventHandler<UserCreatedIntegrationEvent>
{
    public async Task Subscribe(UserCreatedIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        // Handle integration event
        await _mediator.Send(new SyncUserToExternalSystemCommand(integrationEvent.UserId));
    }
}
```

## Generated Analysis Results

### Data Structure

```csharp
public class CodeFlowAnalysisResult
{
    public List<ControllerInfo> Controllers { get; set; } = new();
    public List<CommandInfo> Commands { get; set; } = new();
    public List<EntityInfo> Entities { get; set; } = new();
    public List<DomainEventInfo> DomainEvents { get; set; } = new();
    public List<IntegrationEventInfo> IntegrationEvents { get; set; } = new();
    public List<DomainEventHandlerInfo> DomainEventHandlers { get; set; } = new();
    public List<IntegrationEventHandlerInfo> IntegrationEventHandlers { get; set; } = new();
    public List<IntegrationEventConverterInfo> IntegrationEventConverters { get; set; } = new();
    public List<RelationshipInfo> Relationships { get; set; } = new();
}
```

### Relationship Types

- `MethodToCommand`: Relationships between methods and commands
- `CommandToAggregateMethod`: Relationships between commands and aggregate methods
- `MethodToDomainEvent`: Relationships between methods and domain events
- `DomainEventToIntegrationEvent`: Relationships between domain events and integration events
- `DomainEventToHandler`: Relationships between domain events and handlers
- `IntegrationEventToHandler`: Relationships between integration events and handlers

## Visualization Chart Generation

The framework provides a powerful `MermaidVisualizer` static class that converts analysis results into various types of Mermaid diagrams to help you intuitively understand system architecture and data flow.

### Supported Chart Types

#### 1. Complete Architecture Flow Chart

Generate a complete architecture diagram containing all components and relationships:

```csharp
// Get assemblies to be analyzed
var assemblies = new[] { Assembly.GetExecutingAssembly() }; // Or other assemblies to be analyzed
var analysisResult = AnalysisResultAggregator.Aggregate(assemblies);

// Generate complete architecture flow chart
var architectureChart = MermaidVisualizer.GenerateArchitectureFlowChart(analysisResult);
```

#### 2. Command Flow Chart

Flow chart focused on command execution processes:

```csharp
// Generate command flow chart
var commandChart = MermaidVisualizer.GenerateCommandFlowChart(analysisResult);
```

#### 3. Event Flow Chart

Flow chart focused on event-driven processes:

```csharp
// Generate event flow chart
var eventChart = MermaidVisualizer.GenerateEventFlowChart(analysisResult);
```

#### 4. Class Diagram

Class diagram showing relationships between types:

```csharp
// Generate class diagram
var classDiagram = MermaidVisualizer.GenerateClassDiagram(analysisResult);
```

#### 5. Command Chain Flow Charts

Detailed execution chain diagrams centered on commands:

```csharp
// Generate command chain flow charts collection
var commandChains = MermaidVisualizer.GenerateCommandChainFlowCharts(analysisResult);
foreach (var (chainName, diagram) in commandChains)
{
    Console.WriteLine($"Chain: {chainName}");
    Console.WriteLine(diagram);
}
```

#### 6. Multi-Chain Comprehensive Chart

Show multiple command execution chains in a single diagram:

```csharp
// Generate multi-chain comprehensive chart
var multiChainChart = MermaidVisualizer.GenerateMultiChainFlowChart(analysisResult);
```

**Demo:**

![Multi-Chain Flow Chart Example](../img/GenerateMultiChainFlowChart.png)

#### 7. Independent Chain Chart Collection

Generate independent charts for each chain:

```csharp
// Generate all independent chain charts
var allChainCharts = MermaidVisualizer.GenerateAllChainFlowCharts(analysisResult);
```

**Demo:**

![Independent Chain Charts Example](../img/GenerateAllChainFlowCharts.png)

### Chart Features

- **Automatic Node Classification**: Different types of nodes use different shapes and colors
  - Controllers: Rectangle, blue
  - Commands: Rectangle, purple
  - Aggregate Roots: Diamond, green
  - Domain Events: Circle, orange
  - Integration Events: Rectangle, pink
  - Event Handlers: Rectangle, light green
  - Converters: Trapezoid, blue

- **Smart Relationship Annotation**: Different types of relationships use different arrows and labels
  - Solid arrows: Direct call relationships
  - Dashed arrows: Event handling relationships
  - Thick solid arrows: Important business processes

- **Chain Tracking**: Ability to completely track the full chain from user requests to business execution

## Interactive HTML Visualization

The framework provides the `GenerateVisualizationHtml` method to generate a complete interactive HTML visualization page with built-in chart preview and navigation features.

### HTML Visualization Features

- **Interactive Navigation**: Left sidebar with tree navigation for different chart types
- **Real-time Rendering**: Built-in Mermaid.js for immediate chart visualization
- **Responsive Design**: Adapts to different screen sizes and devices
- **Professional Styling**: Clean, modern interface with dark sidebar and light content area
- **Multi-language Support**: Supports both Chinese and English interfaces

### Generate HTML Visualization

```csharp
using System.Reflection;
using NetCorePal.Extensions.CodeAnalysis;

public class HtmlVisualizationGenerator
{
    public void GenerateVisualizationPage()
    {
        // Get assemblies to be analyzed
        var assemblies = new[] { Assembly.GetExecutingAssembly() };
        var analysisResult = AnalysisResultAggregator.Aggregate(assemblies);
        
        // Generate complete interactive HTML page
        var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult);
        
        // Save to file
        File.WriteAllText("visualization.html", htmlContent);
        
        // Open in browser
        Process.Start(new ProcessStartInfo("visualization.html") { UseShellExecute = true });
    }
}
```

### HTML Page Structure

The generated HTML page includes:

1. **Sidebar Navigation**:
   - Overall Architecture section (Complete architecture flowchart, Class diagram)
   - Specialized Flows section (Command flowchart, Event flowchart)
   - Command Chains section (Individual command execution chains)
   - Multi-Chain Flowchart (Comprehensive view of all chains)
   - Individual Chain Flowcharts (Separate diagrams for each chain)

2. **Main Content Area**:
   - Dynamic chart title and description
   - Interactive Mermaid diagram rendering
   - Responsive layout with loading states and error handling

3. **Interactive Features**:
   - Click navigation items to switch between different charts
   - Active state highlighting for current selection
   - Expandable/collapsible chain sections
   - Counter badges showing number of chains

### Demo HTML Template

You can view a sample HTML visualization at: [MermaidDiagram.html](../../assets/MermaidDiagram.html)

Alternatively, you can download the HTML file and open it directly in your browser for the full interactive experience.

### Customization Options

The generated HTML includes:

- **Modern CSS Styling**: Clean, professional appearance with hover effects
- **Mermaid Configuration**: Optimized theme and layout settings
- **Error Handling**: Graceful handling of rendering errors
- **Loading States**: User-friendly loading indicators

```csharp
// Example of customizing the visualization
var analysisResult = AnalysisResultAggregator.Aggregate(assemblies);
var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult);

// The generated HTML includes all chart types and interactive features
// No additional configuration needed - just save and open in browser
File.WriteAllText("my-architecture-visualization.html", htmlContent);
```

### Browser Compatibility

The generated HTML works with all modern browsers:

- Chrome/Edge 88+
- Firefox 85+
- Safari 14+
- Mobile browsers with JavaScript support

### 在线图表预览功能

You can also preview and edit individual charts online using [Mermaid Live Editor](https://mermaid.live/edit):

1. **Access Mermaid Live Editor**: Open [https://mermaid.live/edit](https://mermaid.live/edit)
2. **Paste Chart Code**: Paste the generated Mermaid code into the editor
3. **Real-time Preview**: The right side will show the rendered chart in real-time
4. **Export Images**: Export charts as PNG, SVG, and other formats
5. **Share Links**: Generate share links for team collaboration

**Usage Steps:**

```csharp
// 1. Generate Mermaid chart code
var analysisResult = AnalysisResultAggregator.Aggregate(assemblies);
var mermaidCode = MermaidVisualizer.GenerateArchitectureFlowChart(analysisResult);

// 2. Print or save to file
Console.WriteLine(mermaidCode);
File.WriteAllText("architecture.mmd", mermaidCode);

// 3. Copy code to https://mermaid.live/edit for preview
```

> **Tip**: The demo images shown above were generated using Mermaid Live Editor. You can paste any Mermaid code generated by the framework into this tool for visual preview.

### Usage Example

```csharp
using System.Reflection;
using NetCorePal.Extensions.CodeAnalysis;

public class ArchitectureAnalyzer
{
    public void GenerateAllDiagrams()
    {
        // Get assemblies to be analyzed
        var assemblies = new[] 
        { 
            Assembly.GetExecutingAssembly(),
            // Can add other assemblies that need to be analyzed
            // typeof(SomeTypeInAnotherAssembly).Assembly
        };
        var analysisResult = AnalysisResultAggregator.Aggregate(assemblies);
        
        // Generate complete architecture chart
        var architectureChart = MermaidVisualizer.GenerateArchitectureFlowChart(analysisResult);
        File.WriteAllText("architecture.md", $"```mermaid\n{architectureChart}\n```");
        
        // Generate command flow chart
        var commandChart = MermaidVisualizer.GenerateCommandFlowChart(analysisResult);
        File.WriteAllText("commands.md", $"```mermaid\n{commandChart}\n```");
        
        // Generate event flow chart
        var eventChart = MermaidVisualizer.GenerateEventFlowChart(analysisResult);
        File.WriteAllText("events.md", $"```mermaid\n{eventChart}\n```");
        
        // Generate class diagram
        var classDiagram = MermaidVisualizer.GenerateClassDiagram(analysisResult);
        File.WriteAllText("classes.md", $"```mermaid\n{classDiagram}\n```");
        
        // Generate command chain flow charts
        var commandChains = MermaidVisualizer.GenerateCommandChainFlowCharts(analysisResult);
        for (int i = 0; i < commandChains.Count; i++)
        {
            var (chainName, diagram) = commandChains[i];
            File.WriteAllText($"chain_{i + 1}.md", $"# {chainName}\n\n```mermaid\n{diagram}\n```");
        }
        
        // Generate multi-chain comprehensive chart
        var multiChainChart = MermaidVisualizer.GenerateMultiChainFlowChart(analysisResult);
        File.WriteAllText("multi-chain.md", $"# System Execution Chain Overview\n\n```mermaid\n{multiChainChart}\n```");
        
        // Generate all independent chain charts
        var allChainCharts = MermaidVisualizer.GenerateAllChainFlowCharts(analysisResult);
        var allChainsMarkdown = "# All System Execution Chains\n\n";
        for (int i = 0; i < allChainCharts.Count; i++)
        {
            allChainsMarkdown += $"## Chain {i + 1}\n\n```mermaid\n{allChainCharts[i]}\n```\n\n";
        }
        File.WriteAllText("all-chains.md", allChainsMarkdown);
    }
}
```

## Real-World Application Scenarios

### 1. Architecture Analysis

- Understand data flow in the system
- Identify potential circular dependencies
- Verify correctness of DDD architecture

### 2. Code Review

- Check adherence to DDD principles
- Ensure correct command and event handling flows
- Verify aggregate root encapsulation

### 3. Documentation Generation

- Auto-generate system architecture diagrams
- Create API documentation
- Generate data flow diagrams
- Create technical presentation slides
- Provide system overview charts for new team members

### 4. Testing Support

- Identify critical testing paths
- Generate test case templates
- Verify business process completeness

## Considerations

1. **Compile-time Analysis**: The source generator runs at compile time, ensure your code compiles successfully
2. **Interface Dependencies**: Ensure relevant types implement framework-defined interfaces
3. **Naming Conventions**: Follow framework naming conventions for optimal analysis results
4. **Performance Considerations**: Large projects may increase compile time

## Troubleshooting

### Common Issues

**Q: Why aren't certain types being recognized?**
A: Ensure types implement the correct interfaces like `ICommand`, `IAggregateRoot`, `IDomainEvent`, etc.

**Q: What if the generated analysis results are inaccurate?**
A: Check if your code follows standard DDD patterns, especially command sending and event handling approaches.

**Q: How can I exclude certain types from analysis?**
A: Currently, the source generator analyzes all qualifying types and doesn't support exclusion functionality.

## More Examples

For more usage examples, please refer to the project's test code and sample projects.
