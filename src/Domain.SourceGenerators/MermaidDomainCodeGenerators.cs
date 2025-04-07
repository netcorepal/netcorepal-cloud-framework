using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace NetCorePal.Extensions.Domain.SourceGenerators
{
    [Generator(LanguageNames.CSharp)]
    public class MermaidDomainCodeGenerators : IIncrementalGenerator
    {
        private static readonly Regex ClassRegex = new(@"class (?<className>\w+)\s*{\s*<<(?<classType>.*?)>>(?<members>[^{]+)}", RegexOptions.Compiled);
        private static readonly Regex ClassFieldRegex = new(@"\+\s*(?<filedType>\w+)\s+(?<filedName>\w+)", RegexOptions.Compiled);
        private static readonly Regex FunctionRegex = new(@"\+\s+(?<filedName>\w+)\s*([(](?<p>.*)?[)])\s*(?<returnType>\w*)");

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var configOptions = context.AnalyzerConfigOptionsProvider
                .Select(static (options, _) => 
                    options.GlobalOptions.TryGetValue("build_property.RootNamespace", out var ns) 
                        ? ns 
                        : null);
            var additionalFilesProvider = context.AdditionalTextsProvider
                .Where(file => file.Path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                .Select((file, cancellationToken) => (file.Path, file.GetText(cancellationToken)?.ToString()));

            var compilationAndFiles = context.CompilationProvider
                .Combine(additionalFilesProvider.Collect())
                .Combine(configOptions);
                // .Combine(configOptions);

            context.RegisterSourceOutput(compilationAndFiles, (spc, right) =>
            {
                // RootNamespace 为空的情况 
                if (string.IsNullOrWhiteSpace(right.Right)) return;
                var nameSpace = right.Right!;
                var files = right.Left.Right;
                foreach (var (filePath, fileContent) in files)
                {
                    if (string.IsNullOrEmpty(fileContent)) continue;

                    var domainName = Path.GetFileNameWithoutExtension(filePath);
                    var matches = ClassRegex.Matches(fileContent);
                    List<string> strongTypeIds = [];

                    foreach (Match match in matches)
                    {
                        var groups = match.Groups;
                        var className = groups["className"].Value;
                        var classType = groups["classType"].Value;
                        var members = groups["members"].Value;

                        var classFields = ClassFieldRegex.Matches(members);

                        if (classType.Equals("DomainEvent", StringComparison.OrdinalIgnoreCase))
                        {
                            var entityName = classFields[0].Groups["filedType"].Value;
                            GenerateDomainEvent(spc,nameSpace, domainName, className, entityName);
                        }
                        else
                        {
                            var idType = string.Empty;
                            StringBuilder memberCode = new();
                            foreach (Match classField in classFields)
                            {
                                var fieldGroups = classField.Groups;
                                var fieldType = fieldGroups["filedType"].Value;
                                var fieldName = fieldGroups["filedName"].Value;

                                if (fieldName.Equals("id", StringComparison.OrdinalIgnoreCase))
                                {
                                    idType = fieldType;
                                    strongTypeIds.Add(fieldType);
                                    continue;
                                }
                                memberCode.AppendLine();
                                memberCode.Append("        ");
                                memberCode.Append($@"public {fieldType} {fieldName} {{ get; protected set; }}");
                            }

                            var functions = FunctionRegex.Matches(members);
                            foreach (Match function in functions)
                            {
                                var functionGroups = function.Groups;
                                var functionName = functionGroups["filedName"].Value;
                                var functionParams = functionGroups["p"].Value;
                                var functionReturnType = functionGroups["returnType"]?.Value;

                                if (functionReturnType != null || functionReturnType == "triggers")
                                {
                                    functionReturnType = "void";
                                }

                                memberCode.AppendLine();
                                memberCode.Append("        ");
                                memberCode.Append($@"public partial {functionReturnType} {functionName}({functionParams});");
                            }

                            GenerateEntity(spc,nameSpace, domainName, className, idType, memberCode.ToString(), classType.Equals("AggregateRoot", StringComparison.OrdinalIgnoreCase));
                        }
                    }

                    foreach (var strongTypeId in strongTypeIds)
                    {
                        string sourceCode = $@"// <auto-generated/>
using NetCorePal.Extensions.Domain;
using System.ComponentModel;
namespace {nameSpace}.{domainName}
{{
    /// <summary>
    /// 订单Id
    /// </summary>
    [TypeConverter(typeof(EntityIdTypeConverter<{strongTypeId}, long>))]
    public record {strongTypeId}(long Id) : IInt64StronglyTypedId
    {{
        public static implicit operator long({strongTypeId} id) => id.Id;
        public static implicit operator {strongTypeId}(long id) => new {strongTypeId}(id);

        public override string ToString()
        {{
            return Id.ToString();
        }}
    }}
}}
";
                        spc.AddSource($"StrongTypeId_{domainName}_{strongTypeId}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
                    }
                }
            });
        }

        void GenerateDomainEvent(SourceProductionContext context, string nameSpace, string domainName, string className, string entityName)
        {
            string source = $@"// <auto-generated/>
using NetCorePal.Extensions.Domain;
using {nameSpace}.{domainName};
namespace {nameSpace}.{domainName}.DomainEvents
{{
    public class {className} : IDomainEvent
    {{
        public {className}({entityName} {ToLowerCamelCase(entityName)})
        {{
            {entityName} = {ToLowerCamelCase(entityName)};
        }}

        public {entityName} {entityName} {{ get; }}
    }}
}}
";
            context.AddSource($"DomainEvents_{className}.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        void GenerateEntity(SourceProductionContext context, string nameSpace, string domainName, string className, string idType, string memberCode, bool isAggregateRoot)
        {
            string source = $@"// <auto-generated/>
using NetCorePal.Extensions.Domain;
namespace {nameSpace}.{domainName}
{{
    public partial class {className} : Entity<{idType}>{(isAggregateRoot ? ", IAggregateRoot" : "")}
    {{
        /// <summary>
        /// 受保护的默认构造函数，用以作为EF Core的反射入口
        /// </summary>
        protected {className}() {{ }}
{memberCode}
    }}
}}
";
            context.AddSource($"{domainName}_{className}.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        string ToLowerCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            if (str.Length <= 1)
            {
                return str.ToLowerInvariant();
            }

            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}