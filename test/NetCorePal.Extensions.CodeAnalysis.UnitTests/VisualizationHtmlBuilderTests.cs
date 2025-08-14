using System.Collections.Generic;
using NetCorePal.Extensions.CodeAnalysis;
using Xunit;

namespace NetCorePal.Extensions.CodeAnalysis.UnitTests;

public class VisualizationHtmlBuilderTests
{
    [Fact]
    public void GenerateVisualizationHtml_ShouldContainBasicHtmlAndDiagrams()
    {
        var result = CodeFlowAnalysisHelper.GetResultFromAssemblies(typeof(VisualizationHtmlBuilderTests).Assembly);

        var controllers = result.Nodes.FindAll(n => n.Type == NodeType.Controller);
        var controllerMethods = result.Nodes.FindAll(n => n.Type == NodeType.ControllerMethod);
        var endpoints = result.Nodes.FindAll(n => n.Type == NodeType.Endpoint);
        var commandSenders = result.Nodes.FindAll(n => n.Type == NodeType.CommandSender);
        var commandSenderMethods = result.Nodes.FindAll(n => n.Type == NodeType.CommandSenderMethod);
        var commands = result.Nodes.FindAll(n => n.Type == NodeType.Command);
        var entities = result.Nodes.FindAll(n => n.Type == NodeType.Aggregate);
        var entityMethods = result.Nodes.FindAll(n => n.Type == NodeType.EntityMethod);
        var domainEvents = result.Nodes.FindAll(n => n.Type == NodeType.DomainEvent);
        var integrationEvents = result.Nodes.FindAll(n => n.Type == NodeType.IntegrationEvent);
        var domainEventHandlers = result.Nodes.FindAll(n => n.Type == NodeType.DomainEventHandler);
        var integrationEventHandlers = result.Nodes.FindAll(n => n.Type == NodeType.IntegrationEventHandler);
        var integrationEventConverters = result.Nodes.FindAll(n => n.Type == NodeType.IntegrationEventConverter);
        
        var html = VisualizationHtmlBuilder.GenerateVisualizationHtml(result);

        //保存到文件
        System.IO.File.WriteAllText("visualization.html", html);

        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("系统模型架构图", html);
        Assert.Contains("mermaid.min.js", html);
    }

    [Fact]
    public void GenerateVisualizationHtml_ShouldContainStatisticsMenuAndData()
    {
        var result = CodeFlowAnalysisHelper.GetResultFromAssemblies(typeof(VisualizationHtmlBuilderTests).Assembly);
        
        var html = VisualizationHtmlBuilder.GenerateVisualizationHtml(result);
        
        // 验证统计信息菜单存在
        Assert.Contains("📊 统计信息", html);
        Assert.Contains("data-diagram=\"Statistics\"", html);
        
        // 验证统计数据被包含在 HTML 中
        Assert.Contains("nodeStats", html);
        Assert.Contains("relationshipStats", html);
        Assert.Contains("totalElements", html);
        Assert.Contains("totalRelationships", html);
        
        // 验证统计信息配置存在
        Assert.Contains("\"Statistics\":{\"title\":'统计信息'", html);
        
        // 验证统计信息被设置为搜索项
        Assert.Contains("统计信息", html);
    }
}
