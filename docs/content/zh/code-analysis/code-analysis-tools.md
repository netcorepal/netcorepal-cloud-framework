# 代码分析工具

NetCorePal.Extensions.CodeAnalysis.Tools 是基于 NetCorePal 代码分析框架的命令行工具，用于从 .NET 程序集生成架构可视化 HTML 文件。

## ⚠️ 重要说明

**工具生效的前提条件**：目标分析的项目/程序集必须引用 `NetCorePal.Extensions.CodeAnalysis` 包。该包包含了源生成器，能够在编译时自动生成代码分析所需的元数据。

```xml
<PackageReference Include="NetCorePal.Extensions.CodeAnalysis" Version="2.8.3" />
```

没有引用此包的程序集将无法生成分析结果。

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

### 智能发现

工具支持自动发现当前目录下的解决方案、项目或程序集：

```bash
# 自动发现并分析当前目录下的所有内容
netcorepal-codeanalysis generate

# 指定解决方案文件
netcorepal-codeanalysis generate --solution MySolution.sln

# 指定项目文件  
netcorepal-codeanalysis generate --project MyProject.csproj

# 指定程序集文件
netcorepal-codeanalysis generate --assembly MyApp.dll
```

### 命令行选项

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
       --configuration Release \
       --output architecture.html \
       --title "我的应用架构"
   ```

3. **分析多个项目：**

   ```bash
   cd MyApp
   netcorepal-codeanalysis generate \
       -p MyApp/MyApp.csproj \
       -p MyApp.Domain/MyApp.Domain.csproj \
       -c Release \
       -o docs/architecture.html
   ```

4. **直接分析程序集：**

   ```bash
   cd MyApp
   netcorepal-codeanalysis generate \
       -a bin/Debug/net8.0/MyApp.dll \
       -a bin/Debug/net8.0/MyApp.Domain.dll \
       --verbose
   ```

## 自动发现机制

工具按以下优先级自动发现项目内容：

1. **解决方案文件**：查找 `*.sln` 文件
2. **项目文件**：查找 `*.csproj` 文件  
3. **程序集文件**：查找 `bin/` 目录下的 `*.dll` 文件

发现规则：

- 在当前目录及子目录中递归搜索
- 解决方案优先于项目，项目优先于程序集
- 自动排除测试项目（包含 "Test"、"Tests" 的项目）
- 自动构建项目并加载生成的程序集

## 系统要求

- .NET 8.0 或更高版本
- 程序集必须包含由 `NetCorePal.Extensions.CodeAnalysis` 源生成器生成的代码分析结果

## 输出内容

工具生成包含以下内容的交互式 HTML 文件：

- **统计信息**：各类型组件的数量统计和分布情况
- **架构总览图**：系统中所有类型及其关系的完整视图
- **处理流程图集合**：每个独立业务链路的流程图（如命令处理链路）
- **聚合关系图集合**：每个聚合根相关的关系图
- **交互式导航**：左侧树形菜单，支持图表类型切换
- **Mermaid Live 集成**：每个图表右上角的"View in Mermaid Live"按钮

## 与构建过程集成

### MSBuild 集成

添加到 `.csproj` 文件：

```xml
<Target Name="GenerateArchitectureVisualization" AfterTargets="Build" Condition="'$(Configuration)' == 'Debug'">
  <Exec Command="netcorepal-codeanalysis generate -a $(OutputPath)$(AssemblyName).dll -o $(OutputPath)architecture.html" 
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
      --output docs/architecture.html \
      --title "MyApp 架构图"
```

## 故障排除

### 常见问题

1. **找不到程序集**：确保程序集文件存在且可访问
2. **无分析结果**：确保程序集使用了 `NetCorePal.Extensions.CodeAnalysis` 包引用进行构建
3. **权限错误**：检查输出目录的写入权限
4. **构建失败**：确保项目可以正常构建，检查依赖项

### 详细输出

使用 `--verbose` 标志获取分析过程的详细信息：

```bash
netcorepal-codeanalysis generate --verbose
```

这将显示：

- 发现的文件和项目
- 构建过程信息
- 加载的程序集
- 分析统计信息
- 文件生成详情
- 发生问题时的错误详情

## 相关包

- [`NetCorePal.Extensions.CodeAnalysis`](../code-flow-analysis.md)：核心分析框架
- 源生成器：用于自动分析的源生成器
