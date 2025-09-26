# NetCorePal 框架性能测试

本项目包含了 NetCorePal Cloud Framework 的性能测试，使用 BenchmarkDotNet 进行精确的性能测量和统计分析。

## 测试类别

### 1. 框架核心性能 (Framework)
- **DependencyInjectionBenchmark**: 依赖注入性能测试
  - 服务解析性能（Scoped、Singleton、Transient）
  - 多服务批量解析性能
- **EntityIdBenchmark**: EntityId 操作性能测试
  - 创建、解析、转换、比较等操作性能
- **JsonSerializationBenchmark**: JSON 序列化性能测试
  - System.Text.Json vs Newtonsoft.Json 性能对比

### 2. 领域模型性能 (Domain)  
- **AggregateRootBenchmark**: 聚合根操作性能测试
  - 聚合根创建和更新性能
  - 领域事件处理性能

### 3. 数据库操作性能 (Database)
- **RepositoryBenchmark**: 仓储模式性能测试
  - 实体查询、添加、更新性能
  - EF Core 不同查询方式性能对比

## 运行性能测试

### 运行所有测试
```bash
cd test/NetCorePal.Extensions.Performance.Tests
dotnet run -c Release
```

### 运行特定测试类
```bash
# 运行依赖注入测试
dotnet run -c Release --filter "*DependencyInjectionBenchmark*"

# 运行 EntityId 测试
dotnet run -c Release --filter "*EntityIdBenchmark*"

# 运行仓储测试
dotnet run -c Release --filter "*RepositoryBenchmark*"
```

### 运行特定测试方法
```bash
# 运行特定方法
dotnet run -c Release --filter "*DependencyInjectionBenchmark.ResolveScoped*"
```

## 测试输出

性能测试结果会以多种格式输出：

- **控制台输出**: 实时查看测试进度和简要结果
- **Markdown 报告**: `BenchmarkDotNet.Artifacts/results/*.md` - 适合文档和分享
- **CSV 文件**: `BenchmarkDotNet.Artifacts/results/*.csv` - 适合数据分析
- **HTML 报告**: `BenchmarkDotNet.Artifacts/results/*.html` - 适合可视化查看

## 测试配置

测试使用自定义配置 (`CustomBenchmarkConfig`)：

- **目标框架**: .NET 8.0
- **预热次数**: 3 次
- **测试迭代**: 10 次（最少 5 次，最多 20 次）
- **内存诊断**: 启用（显示内存分配情况）
- **统计指标**: 平均值、中位数、标准差、P95 百分位数

## 性能指标说明

| 指标 | 说明 |
|------|------|
| Mean | 平均执行时间 |
| Median | 中位数执行时间 |
| StdDev | 标准差（稳定性指标） |
| P95 | 95% 的请求在此时间内完成 |
| Allocated | 每次操作分配的内存 |
| Ratio | 相对于基准测试的性能比率 |

## 性能目标

### 框架核心操作
- 依赖注入服务解析: < 1μs (微秒)
- EntityId 操作: < 100ns (纳秒)
- JSON 序列化/反序列化: < 10μs

### 领域操作
- 聚合根创建: < 1μs
- 领域事件添加: < 500ns

### 数据库操作 (内存数据库)
- 单条记录查询: < 100μs
- 批量查询 (10条): < 500μs
- 单条记录插入: < 200μs

## 注意事项

1. **发布模式**: 性能测试必须在 Release 模式下运行，Debug 模式结果不准确
2. **环境一致性**: 确保测试环境稳定，避免其他程序影响测试结果
3. **多次运行**: 建议多次运行测试以确认结果一致性
4. **基准对比**: 使用 `[Benchmark(Baseline = true)]` 标记的方法作为基准进行性能对比

## 扩展性能测试

要添加新的性能测试：

1. 在相应目录下创建新的测试类
2. 使用 `[MemoryDiagnoser]` 和 `[SimpleJob]` 特性
3. 使用 `[Benchmark]` 标记测试方法
4. 使用 `[GlobalSetup]` 进行测试初始化
5. 使用 `[GlobalCleanup]` 进行资源清理

## 性能回归检测

建议将性能测试集成到 CI/CD 流程中：

1. 定期运行性能测试
2. 比较不同版本的性能指标
3. 设置性能阈值警报
4. 记录性能基线数据

## 相关资源

- [BenchmarkDotNet 官方文档](https://benchmarkdotnet.org/)
- [.NET 性能优化指南](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/performance-warnings)
- [NetCorePal Framework 文档](https://github.com/netcorepal/netcorepal-cloud-framework)