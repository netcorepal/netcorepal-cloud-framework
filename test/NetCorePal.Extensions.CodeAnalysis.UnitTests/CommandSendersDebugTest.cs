using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests
{
    public class CommandSendersDebugTest
    {
        private readonly ITestOutputHelper _output;

        public CommandSendersDebugTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Should_Have_CommandSenders_Data()
        {
            // Arrange
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName?.Contains("NetCorePal.Extensions.CodeAnalysis.UnitTests") == true)
                .ToArray();
            var result = AnalysisResultAggregator.Aggregate(assemblies);

            // Act & Assert
            _output.WriteLine($"Controllers count: {result.Controllers.Count}");
            _output.WriteLine($"CommandSenders count: {result.CommandSenders.Count}");
            _output.WriteLine($"Commands count: {result.Commands.Count}");
            _output.WriteLine($"Relationships count: {result.Relationships.Count}");

            _output.WriteLine("\n=== Controllers ===");
            foreach (var controller in result.Controllers)
            {
                _output.WriteLine($"- {controller.Name} ({controller.FullName})");
            }

            _output.WriteLine("\n=== CommandSenders ===");
            foreach (var sender in result.CommandSenders)
            {
                _output.WriteLine($"- {sender.Name} ({sender.FullName})");
                foreach (var method in sender.Methods)
                {
                    _output.WriteLine($"  - {method}");
                }
            }

            _output.WriteLine("\n=== MethodToCommand Relationships ===");
            foreach (var rel in result.Relationships.Where(r => r.CallType == "MethodToCommand"))
            {
                _output.WriteLine($"- {rel.SourceType}.{rel.SourceMethod} -> {rel.TargetType}");
            }
            
            // 额外的调试信息：显示所有关系中的发送者类型
            _output.WriteLine("\n=== All Command Senders from Relationships ===");
            var senderTypes = result.Relationships
                .Where(r => r.CallType == "MethodToCommand")
                .Select(r => r.SourceType)
                .Distinct()
                .OrderBy(s => s);
            
            foreach (var senderType in senderTypes)
            {
                var isController = (senderType.Contains("Controller") || senderType.Contains("Endpoint")) && !senderType.Contains("Handler");
                _output.WriteLine($"- {senderType} (IsController: {isController})");
            }        // 确保有 CommandSenders 数据
        _output.WriteLine($"\n=== Direct IAnalysisResult Check ===");
        
        // 直接检查源生成器生成的 IAnalysisResult 实现
        var analysisResultTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => typeof(IAnalysisResult).IsAssignableFrom(type) 
                           && !type.IsInterface 
                           && !type.IsAbstract)
            .ToList();

        foreach (var type in analysisResultTypes)
        {
            _output.WriteLine($"Found IAnalysisResult: {type.FullName}");
            try
            {
                var instance = Activator.CreateInstance(type) as IAnalysisResult;
                var directResult = instance?.GetResult();
                if (directResult != null)
                {
                    _output.WriteLine($"  Controllers: {directResult.Controllers.Count}");
                    _output.WriteLine($"  CommandSenders: {directResult.CommandSenders.Count}");
                    _output.WriteLine($"  Commands: {directResult.Commands.Count}");
                    _output.WriteLine($"  Relationships: {directResult.Relationships.Count}");
                    
                    if (directResult.CommandSenders.Count > 0)
                    {
                        _output.WriteLine("  CommandSenders details:");
                        foreach (var cs in directResult.CommandSenders)
                        {
                            _output.WriteLine($"    - {cs.Name} ({cs.FullName})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"  Error creating instance: {ex.Message}");
            }
        }
        
        Assert.True(result.CommandSenders.Count > 0, "CommandSenders should contain data");
            
            // 确保 OrderProcessingService 在 CommandSenders 中
            var orderProcessingService = result.CommandSenders.FirstOrDefault(cs => cs.FullName.Contains("OrderProcessingService"));
            Assert.NotNull(orderProcessingService);
            Assert.True(orderProcessingService.Methods.Count > 0, "OrderProcessingService should have methods");
        }
    }
}
