# 配置服务

这里介绍了如何在`Program.cs`文件中配置服务。

```csharp
//配置服务
using Microsoft.Extensions.DependencyInjection;
using NetCorePal.Extensions.DependencyInjection;
using YourNamespace;

var builder = WebApplication.CreateBuilder(args);

```
