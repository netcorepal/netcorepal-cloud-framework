using System;
using System.Text;

namespace NetCorePal.Extensions.CodeAnalysis
{
    /// <summary>
    /// 负责生成架构可视化 HTML 页面及相关样式
    /// </summary>
    public static class VisualizationHtmlBuilder
    {
        public static string GenerateVisualizationHtml(CodeFlowAnalysisResult analysisResult,
            string title = "NetCorePal 架构图可视化")
        {
            // 生成所有类型的图表，直接调用各 Visualizer
            var architectureOverviewMermaid =
                MermaidVisualizers.ArchitectureOverviewMermaidVisualizer.GenerateMermaid(analysisResult);
            var allProcessingFlowMermaid =
                MermaidVisualizers.ProcessingFlowMermaidVisualizer.GenerateMermaid(analysisResult);
            var allAggregateMermaid =
                MermaidVisualizers.AggregateRelationMermaidVisualizer.GenerateAllAggregateMermaid(analysisResult);

            // 读取嵌入资源模板内容
            var assembly = typeof(VisualizationHtmlBuilder).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("visualization-template.html", StringComparison.OrdinalIgnoreCase));
            if (resourceName == null)
            {
                throw new InvalidOperationException(
                    $"未找到嵌入的 visualization-template.html 资源。可用资源: {string.Join(", ", assembly.GetManifestResourceNames())}");
            }

            string template;
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException($"无法获取资源流: {resourceName}");
                }

                using (var reader = new System.IO.StreamReader(stream))
                {
                    template = reader.ReadToEnd();
                }
            }

            // 构建各部分内容
            var analysisResultJson = BuildAnalysisResultJson(analysisResult);
            var diagramConfigsJson = BuildDiagramConfigsJson();
            var diagramsJson = BuildArchitectureOverviewMermaidJson(architectureOverviewMermaid);
            var allChainFlowChartsJson = BuildProcessingFlowMermaidJson(allProcessingFlowMermaid);
            var allAggregateRelationDiagramsJson = BuildAllAggregateRelationDiagramsJson(allAggregateMermaid);

            // 替换模板中的占位符
            var html = template
                .Replace("{{TITLE}}", EscapeHtml(title))
                .Replace("{{ANALYSIS_RESULT}}", analysisResultJson)
                .Replace("{{DIAGRAM_CONFIGS}}", diagramConfigsJson)
                .Replace("{{DIAGRAMS}}", diagramsJson)
                .Replace("{{ALL_CHAIN_FLOW_CHARTS}}", allChainFlowChartsJson)
                .Replace("{{ALL_AGGREGATE_RELATION_DIAGRAMS}}", allAggregateRelationDiagramsJson);

            return html;
        }

        // 构建 analysisResult 的 JSON 字符串
        private static string BuildAnalysisResultJson(CodeFlowAnalysisResult analysisResult)
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"nodes\":[");
            for (int i = 0; i < analysisResult.Nodes.Count; i++)
            {
                var node = analysisResult.Nodes[i];
                string nodeTypeStr = node.Type.ToString();
                sb.Append(
                    $"{{\"id\":\"{EscapeJavaScript(node.Id ?? string.Empty)}\",\"name\":\"{EscapeJavaScript(node.Name ?? string.Empty)}\",\"fullName\":\"{EscapeJavaScript(node.FullName ?? string.Empty)}\",\"type\":\"{EscapeJavaScript(nodeTypeStr)}\"}}");
                if (i < analysisResult.Nodes.Count - 1) sb.Append(",");
            }

            sb.Append("],\"relationships\":[");
            for (int i = 0; i < analysisResult.Relationships.Count; i++)
            {
                var rel = analysisResult.Relationships[i];
                string relTypeStr = rel.Type.ToString();
                sb.Append(
                    $"{{\"from\":\"{EscapeJavaScript(rel.FromNode?.Id ?? string.Empty)}\",\"to\":\"{EscapeJavaScript(rel.ToNode?.Id ?? string.Empty)}\",\"type\":\"{EscapeJavaScript(relTypeStr)}\"}}");
                if (i < analysisResult.Relationships.Count - 1) sb.Append(",");
            }

            sb.Append("]}");
            return sb.ToString();
        }

        // 构建 diagramConfigs 的 JSON 字符串
        private static string BuildDiagramConfigsJson()
        {
            return "{" +
                   "\"class\":{\"title\":'架构大图',\"description\":'展示系统中所有类型及其关系的完整视图'}," +
                   "\"command\":{\"title\":'命令关系图',\"description\":'展示命令在系统中的完整流转与关系'}" +
                   "}";
        }

        // 构建 diagrams 的 JSON 字符串
        private static string BuildArchitectureOverviewMermaidJson(string classDiagram)
        {
            return $"{{\"ArchitectureOverview\":`{EscapeJavaScriptTemplate(classDiagram)}`}}";
        }

        // 构建 allChainFlowCharts 的 JSON 字符串
        private static string BuildProcessingFlowMermaidJson(
            System.Collections.Generic.List<(string ChainName, string Diagram)> allProcessingFlowDiagrams)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < allProcessingFlowDiagrams.Count; i++)
            {
                var (chainName, diagram) = allProcessingFlowDiagrams[i];
                sb.Append(
                    $"{{\"name\":\"{EscapeJavaScript(chainName)}\",\"diagram\":`{EscapeJavaScriptTemplate(diagram)}`}}");
                if (i < allProcessingFlowDiagrams.Count - 1) sb.Append(",");
            }

            sb.Append("]");
            return sb.ToString();
        }

        // 构建 allAggregateRelationDiagrams 的 JSON 字符串
        private static string BuildAllAggregateRelationDiagramsJson(
            System.Collections.Generic.List<(string AggregateName, string Diagram)> allAggregateRelationDiagrams)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < allAggregateRelationDiagrams.Count; i++)
            {
                var (aggName, diagram) = allAggregateRelationDiagrams[i];
                sb.Append(
                    $"{{\"name\":\"{EscapeJavaScript(aggName)}\",\"diagram\":`{EscapeJavaScriptTemplate(diagram)}`}}");
                if (i < allAggregateRelationDiagrams.Count - 1) sb.Append(",");
            }

            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// HTML转义
        /// </summary>
        private static string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#39;");
        }

        /// <summary>
        /// JavaScript字符串转义
        /// </summary>
        private static string EscapeJavaScript(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("'", "\\'")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("<", "\\u003c")
                .Replace(">", "\\u003e");
        }

        /// <summary>
        /// JavaScript模板字符串转义
        /// </summary>
        private static string EscapeJavaScriptTemplate(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Replace("\\", "\\\\")
                .Replace("`", "\\`")
                .Replace("${", "\\${");
        }
    }
}