# NetCorePal.Extensions.CodeAnalysis.Tools

基于 NetCorePal 代码分析框架的命令行工具，用于从 .NET 程序集生成架构可视化 HTML 文件。

## 快速开始

### 安装

```bash
dotnet tool install -g NetCorePal.Extensions.CodeAnalysis.Tools
```

### 使用

```bash
# 进入项目目录
cd MyApp

# 自动发现并分析当前目录下的所有内容
netcorepal-codeanalysis generate

# 指定解决方案文件
netcorepal-codeanalysis generate --solution MySolution.sln

# 自定义输出和标题
netcorepal-codeanalysis generate --output my-architecture.html --title "我的架构图"
```

## 系统要求

- .NET 8.0 或更高版本
- 程序集必须包含由 `NetCorePal.Extensions.CodeAnalysis` 源生成器生成的代码分析结果

## 完整文档

详细的使用说明、命令行选项、集成方式和故障排除，请参阅：

- [中文文档](https://netcorepal.github.io/netcorepal-cloud-framework/zh/code-analysis/code-analysis-tools/)
- [English Documentation](https://netcorepal.github.io/netcorepal-cloud-framework/en/code-analysis/code-analysis-tools/)

## 相关包

- [`NetCorePal.Extensions.CodeAnalysis`](../NetCorePal.Extensions.CodeAnalysis/)：核心分析框架
- [`NetCorePal.Extensions.CodeAnalysis.SourceGenerators`](../NetCorePal.Extensions.CodeAnalysis.SourceGenerators/)：用于自动分析的源生成器
