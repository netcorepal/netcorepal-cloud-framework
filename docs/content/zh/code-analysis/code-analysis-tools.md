# 代码分析工具

NetCorePal.Extensions.CodeAnalysis.Tools 是基于 NetCorePal 代码分析框架的命令行工具，用于从 .NET 项目生成交互式架构可视化 HTML 文件（基于 .NET 10 单文件执行）。

## ⚠️ 重要说明

**工具生效的前提条件**：目标分析的项目/程序集必须引用 `NetCorePal.Extensions.CodeAnalysis` 包。该包包含了源生成器，能够在编译时自动生成代码分析所需的元数据。

```xml
<PackageReference Include="NetCorePal.Extensions.CodeAnalysis" />
```

没有引用此包的项目将无法生成分析结果。

## 安装

作为全局 dotnet 工具安装：

```bash
dotnet tool install -g NetCorePal.Extensions.CodeAnalysis.Tools
```

或在项目中本地安装：

```bash
dotnet tool install NetCorePal.Extensions.CodeAnalysis.Tools
```

## 使用方法

### 命令概览

工具提供两个主要命令：

| 命令 | 说明 |
|------|------|
| `generate` | 分析项目/解决方案并生成交互式 HTML 可视化 |
| `snapshot` | 管理架构快照以追踪演进历史 |

**快速参考**：

```bash
# 生成架构可视化
netcorepal-codeanalysis generate [选项]

# 创建架构快照
netcorepal-codeanalysis snapshot add [选项]
```

### 快速上手

```bash
# 进入项目目录
cd MyApp

# 自动发现并分析当前目录下的解决方案或项目
netcorepal-codeanalysis generate

# 指定解决方案文件（.sln/.slnx）
netcorepal-codeanalysis generate --solution MySolution.sln

# 指定项目文件（可多次指定）
netcorepal-codeanalysis generate --project MyProject.csproj

# 自定义输出文件和标题
netcorepal-codeanalysis generate --output my-architecture.html --title "我的架构图"

# 启用详细输出
netcorepal-codeanalysis generate --verbose
```

### 命令参数

| 选项 | 别名 | 类型 | 默认值 | 说明 |
|---|---|---|---|---|
| `--solution <solution>` | `-s` | 文件路径 | 无 | 要分析的解决方案文件，支持 `.sln`/`.slnx` |
| `--project <project>` | `-p` | 文件路径（可多次） | 无 | 要分析的项目文件（`.csproj`），可重复指定多个 |
| `--output <output>` | `-o` | 文件路径 | `architecture-visualization.html` | 输出的 HTML 文件路径 |
| `--format <format>` | `-f` | 字符串 | `html` | 输出格式：`html` 或 `markdown`（也可使用 `md`） |
| `--title <title>` | `-t` | 字符串 | `架构可视化` | 生成页面/文档的标题 |
| `--verbose` | `-v` | 开关 | `false` | 启用详细日志输出 |
| `--include-tests` | 无 | 开关 | `false` | 包含测试项目（默认不包含；规则见下文“测试项目识别规则”） |

#### `generate` 命令

**输入源选项（按优先级排序）：**

- `--assembly, -a`：指定程序集文件 (.dll)。可多次指定
- `--project, -p`：指定项目文件 (.csproj)。可多次指定  
- `--solution, -s`：指定解决方案文件 (.sln)。可多次指定

**构建选项：**

- `--configuration, -c`：构建配置 (Debug/Release)。默认：Debug

**输出选项：**

- `--output, -o`：输出 HTML 文件路径。默认：code-analysis.html
- `--title, -t`：HTML 页面标题。默认：Architecture Visualization
- `--verbose, -v`：启用详细输出用于调试

### 使用示例

1. **自动发现分析：**

   ```bash
   # 进入项目目录
   cd MyApp
   
   # 自动发现并分析当前目录下的解决方案/项目/程序集
   netcorepal-codeanalysis generate
   
   # 自动发现并指定输出文件
   netcorepal-codeanalysis generate -o my-architecture.html
   ```

2. **分析特定解决方案：**

   ```bash
   cd MyApp
      netcorepal-codeanalysis generate \
         --solution MyApp.sln \
         --output architecture.html \
         --title "我的应用架构"
   ```

3. **分析多个项目：**

   ```bash
   cd MyApp
      netcorepal-codeanalysis generate \
         -p MyApp/MyApp.csproj \
         -p MyApp.Domain/MyApp.Domain.csproj \
         -o docs/architecture.html
   ```

   


## 输出格式

工具支持两种输出格式：

### HTML 格式（默认）

生成交互式 HTML 可视化页面，包含完整的导航、图表切换和在线编辑功能。

```bash
# 生成 HTML 格式（默认）
netcorepal-codeanalysis generate

# 显式指定 HTML 格式
netcorepal-codeanalysis generate --format html
```

### Markdown 格式

生成 Markdown 文档，便于大语言模型理解业务模型，包含：
- 概览统计（节点和关系计数）
- 架构元素列表（按类型分组）
- 组件关系详情
- 嵌入式 Mermaid 图表（架构总览图、处理流程图、聚合关系图）
- 版本历史趋势（如果启用历史记录）

```bash
# 生成 Markdown 格式
netcorepal-codeanalysis generate --format markdown

# 或使用简写
netcorepal-codeanalysis generate --format md

# 自定义输出文件名
netcorepal-codeanalysis generate --format markdown --output architecture.md

# 自定义标题
netcorepal-codeanalysis generate --format md --title "订单系统架构分析"
```

**注意**：当使用默认输出文件名时，Markdown 格式会自动将文件扩展名改为 `.md`。

## 自动发现机制

当未提供 `--solution` 与 `--project` 时，工具会在“当前目录（顶层）”自动发现分析目标：

- 优先级：`.slnx` > `.sln` > 顶层 `*.csproj`
- 非递归扫描目录：仅加载当前目录顶层的解决方案/项目文件，随后递归分析其依赖项目
- 默认排除测试项目：除非显式传入 `--include-tests`
- 输出可见性：
   - 选择 `.slnx/.sln` 会打印 `Using solution (...): <文件名>`；随后打印“Projects to analyze (N)”并列出递归依赖在内的完整项目清单
   - 选择顶层 `*.csproj` 会直接打印“Projects to analyze (N)”并列出包含递归依赖的完整清单

> 说明：工具会在隔离的临时工作目录中生成并执行动态 `app.cs`，并使用 `--no-launch-profile` 运行以避免继承当前目录的 `launchSettings.json`/`global.json` 等环境影响。

### 测试项目识别规则

- 默认行为：测试项目会被排除在分析之外（除非显式传入 `--include-tests`）
- 判定规则（满足任一即视为测试项目）：
   - 项目文件所在路径的任一父级目录名为 `test` 或 `tests`（不区分大小写）
   - 项目文件（.csproj）中包含 `<IsTestProject>true</IsTestProject>`（大小写与空白不敏感）

## 系统要求

- 运行环境：.NET 10 或更高版本 SDK（单文件执行依赖 .NET 10 特性）
- 被分析项目的目标框架：支持 `net8.0`、`net9.0`、`net10.0` 和 `net11.0`
- 被分析项目必须引用 `NetCorePal.Extensions.CodeAnalysis` 包（包含源生成器）

## 输出内容

工具生成包含以下内容的交互式 HTML 文件：

- **统计信息**：各类型组件的数量统计和分布情况
- **架构总览图**：系统中所有类型及其关系的完整视图
- **处理流程图集合**：每个独立业务链路的流程图（如命令处理链路）
- **聚合关系图集合**：每个聚合根相关的关系图
- **交互式导航**：左侧树形菜单，支持图表类型切换
- **Mermaid Live 集成**：每个图表右上角的"View in Mermaid Live"按钮
- **📊 版本历史功能**（如果存在快照）：
  - **版本选择器**：交互式下拉框切换不同快照版本
  - **历史趋势图表**（2个或更多快照）：
    - 总体趋势图（总元素和总关系数量）
    - 元素类型趋势图（各类型元素数量变化）
    - 关系类型趋势图（各类型关系数量变化）
  - **交互式图例**：点击显示/隐藏特定指标
  - **Chart.js 可视化**：专业的响应式图表
  - **过滤一致性**：趋势图与统计信息使用相同的过滤规则

## 与构建过程集成

### MSBuild 集成

添加到 `.csproj` 文件：

```xml
<Target Name="GenerateArchitectureVisualization" AfterTargets="Build" Condition="'$(Configuration)' == 'Debug'">
   <Exec Command="netcorepal-codeanalysis generate --project $(MSBuildProjectFullPath) --output $(OutputPath)architecture-visualization.html" 
            ContinueOnError="true" />
</Target>
```

### GitHub Actions

添加到工作流程：

```yaml
- name: Generate Architecture Visualization
  run: |
    dotnet tool install -g NetCorePal.Extensions.CodeAnalysis.Tools
    cd MyApp
      netcorepal-codeanalysis generate \
         --output docs/architecture-visualization.html \
         --title "MyApp 架构图"
```

## 故障排除

### 常见问题

1. **找不到项目/解决方案**：确保路径正确且文件存在
2. **无分析结果**：确保项目引用了 `NetCorePal.Extensions.CodeAnalysis` 包并能正常编译
3. **权限错误**：检查输出目录的写入权限
4. **构建失败**：确保项目可以正常构建，检查依赖项

### 详细输出

使用 `--verbose` 标志获取分析过程的详细信息：

```bash
netcorepal-codeanalysis generate --verbose
```

这将显示：

- 发现的文件和项目
- 递归依赖收集信息
- 单文件执行过程日志
- 分析统计信息
- 文件生成详情
- 发生问题时的错误详情

## 相关包

- [`NetCorePal.Extensions.CodeAnalysis`](../code-flow-analysis.md)：核心分析框架
- 源生成器：用于自动分析的源生成器

## 历史记录特性（类似 EF Core 迁移）

工具提供了版本快照功能，可以追踪架构的演进历史，类似于 Entity Framework Core 的迁移机制。

**快照以C#代码文件形式保存**，类似EF Core的迁移快照，便于版本控制和代码审查。

### 创建快照

```bash
# 在当前目录创建项目架构快照（自动发现项目）
netcorepal-codeanalysis snapshot add --description "初始版本"

# 指定项目文件创建快照
netcorepal-codeanalysis snapshot add --project MyProject.csproj --description "添加订单模块"

# 指定快照名称（EF Core 风格）
netcorepal-codeanalysis snapshot add --project MyProject.csproj --name "AddedPaymentFeature" --description "添加支付功能"
```

**快照文件命名规则**：
- 格式：`Snapshot_{Version}_{Name}.cs`
- Version：时间戳格式 `YYYYMMDDHHmmss`
- Name：可选，从 `--name` 或 `--description` 派生（sanitized为有效标识符）
- 示例：`Snapshot_20260116120000_AddedOrderModule.cs`

**生成的快照类**：
```csharp
// <auto-generated />
// Snapshot created: 2026-01-16 12:00:00
// Description: 添加订单模块

using NetCorePal.Extensions.CodeAnalysis.Snapshots;
using NetCorePal.Extensions.CodeAnalysis.Attributes;

namespace CodeAnalysisSnapshots
{
    public partial class Snapshot_20260116120000_AddedOrderModule : CodeFlowAnalysisSnapshot
    {
        public Snapshot_20260116120000_AddedOrderModule()
        {
            Metadata = new SnapshotMetadata
            {
                Version = "20260116120000",
                Timestamp = "2026-01-16 12:00:00",
                Description = "添加订单模块",
                // ...
            };
            
            MetadataAttributes = new MetadataAttribute[]
            {
                new EntityMetadataAttribute("MyApp.Domain.Order", true, 
                    new string[] { "OrderItem" }, 
                    new string[] { "Create" }),
                // ... 所有其他元数据
            };
        }
    }
}
```

### 生成带历史记录的HTML

```bash
# 默认生成包含历史快照的交互式HTML（自动通过反射发现快照）
netcorepal-codeanalysis generate

# 禁用历史记录功能
netcorepal-codeanalysis generate --no-history
```

**快照发现机制**（反射based）：
- ✅ 自动从项目程序集中发现所有快照类（继承自 `CodeFlowAnalysisSnapshot`）
- ✅ 只有代码变化时才会添加新快照（基于 hash 比较）
- ✅ 快照按版本自动排序（最新的在前）
- ✅ 无需手动指定快照目录

**生成的HTML功能**：

1. **版本选择器**（多个快照时显示）
   - 交互式下拉框，显示快照描述和时间戳
   - 切换版本时自动刷新所有图表和统计
   - 专业深色主题样式

2. **历史趋势图表**（2个或更多快照时显示）
   - **总体趋势图**：总元素和总关系数量随时间变化
   - **元素类型趋势图**：Aggregate、Command、DomainEvent等各类型数量变化
   - **关系类型趋势图**：CommandToHandler、AggregateToDomainEvent等关系变化
   - **交互式图例**：点击图例项显示/隐藏对应指标
   - **响应式图表**：基于 Chart.js，支持缩放和详细提示
   - **时间轴顺序**：X轴从左到右按时间顺序排列（最早→最新）
   - **过滤一致性**：使用与统计信息页面相同的过滤规则

3. **版本间同步**
   - 所有视图（统计、架构图、流程图）在不同快照间自动同步
   - 保持一致的用户体验

### 典型工作流程

```bash
# 1. 初始架构快照
netcorepal-codeanalysis snapshot add --project MyProject.csproj --description "项目初始版本"

# 2. 开发新功能...

# 3. 创建新快照
netcorepal-codeanalysis snapshot add --project MyProject.csproj --description "添加支付功能"

# 4. 生成可视化HTML（默认包含历史，通过反射自动发现所有快照）
netcorepal-codeanalysis generate --project MyProject.csproj --output architecture.html

# 5. 打开生成的HTML文件查看：
#    - 版本选择器下拉框（切换不同快照）
#    - 历史趋势图表（2个或更多快照时显示）
#    - 完整的架构分析和统计信息

# 6. 提交快照到版本控制（推荐）
git add Snapshots/
git commit -m "Add architecture snapshot: 添加支付功能"
```

### 版本控制集成

快照以 C# 代码文件形式保存，建议将其提交到版本控制系统：

```bash
# 将快照目录添加到版本控制
git add Snapshots/
git commit -m "Add architecture snapshot: [描述]"
```

**优势**：
- ✅ 类型安全，编译时检查
- ✅ 易于代码审查和diff
- ✅ 自然集成到Git工作流
- ✅ 遵循EF Core迁移的最佳实践
- ✅ 支持多人协作（合并冲突可见且易于解决）

### 快照命令参考

#### 主要命令

| 命令 | 说明 |
|------|------|
| `netcorepal-codeanalysis snapshot` | 管理分析快照（类似 EF Core 迁移） |
| `netcorepal-codeanalysis snapshot add` | 创建当前分析的新快照 |

#### `snapshot add` 命令参数

| 选项 | 别名 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| `--project <project>` | `-p` | 文件路径 | 自动发现 | 要分析的项目文件（`.csproj`） |
| `--name <name>` | `-n` | 字符串 | 无 | 快照名称（可选，用于文件名，建议使用英文标识符，如：InitialCreate） |
| `--description <description>` | `-d` | 字符串 | "Snapshot created" | 快照描述（可使用中文） |
| `--snapshot-dir <dir>` | — | 目录路径 | `Snapshots` | 快照存储目录 |
| `--verbose` | `-v` | 开关 | `false` | 启用详细输出 |
| `--include-tests` | — | 开关 | `false` | 包含测试项目 |

**命令用法**：

```bash
# 创建带描述的快照
netcorepal-codeanalysis snapshot add --description "初始版本"

# 为特定项目创建快照
netcorepal-codeanalysis snapshot add --project MyProject.csproj --description "添加订单模块"

# 创建带自定义名称和目录的快照
netcorepal-codeanalysis snapshot add \
  --project MyProject.csproj \
  --name "AddedPaymentFeature" \
  --description "添加支付功能" \
  --snapshot-dir ./MySnapshots
```

**注意**：
- `snapshot add` 仅支持单个项目文件（不支持解决方案）
- 如果当前目录只有一个 `.csproj` 文件，可以省略 `--project`
- 快照文件保存在项目目录中（相对路径相对于项目目录解析）
- 快照以 C# 代码文件形式保存
- `--name` 参数建议使用英文标识符，`--description` 可使用中文描述

### 查看快照历史

快照历史无需通过CLI命令查看，而是通过生成的HTML可视化文件查看：

```bash
# 生成包含所有快照历史的HTML
netcorepal-codeanalysis generate --project MyProject.csproj --output architecture.html
```

生成的HTML文件提供了更好的快照查看体验：
- **版本选择器下拉框**：交互式切换不同快照版本
- **历史趋势图表**：可视化展示架构演进（2个或更多快照时显示）
  - 总体趋势图（元素和关系数量变化）
  - 元素类型趋势图（各类型元素数量变化）
  - 关系类型趋势图（各类型关系数量变化）
- **交互式图例**：点击显示/隐藏特定指标
- **完整统计信息**：每个快照的详细元数据和统计数据

**快照自动发现**：
- 工具通过反射自动从项目程序集中发现所有快照类
- 无需手动指定快照目录或版本号
- 只有代码实际变化（hash不同）时才会创建新快照
