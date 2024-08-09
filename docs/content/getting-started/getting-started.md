# 入门

## 环境准备

### 安装编程工具

选择下列其中一种编程工具

- [Visual Studio Code](https://code.visualstudio.com/)
- [Visual Studio](https://visualstudio.microsoft.com/)
- [JetBrains Rider](https://www.jetbrains.com/rider/)

### 安装 .NET SDK

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### 安装 Docker 环境

拥有Docker环境，用于自动化单元测试和集成测试。

Docker Desktop下载地址： （https://www.docker.com/products/docker-desktop/)

## 安装工程模版工具

正式版 [NetCorePal.Template - NuGet](https://www.nuget.org/packages/NetCorePal.Template)

```shell
dotnet new install NetCorePal.Template
```

or

预览版本 [NetCorePal.Template - MyGet](https://www.myget.org/feed/netcorepal/package/nuget/NetCorePal.Template)

```shell
dotnet new install NetCorePal.Template::<package-version> --add-source "https://www.myget.org/F/netcorepal/api/v3/index.json"
```

## 创建工程

```shell
dotnet new netcorepal-web -n MyWebApp
```

## 运行项目

```shell
cd MyWebApp
# Todo 添加构建基础设施的脚本
dotnet run
```

## 运行测试

```shell
dotnet test
```