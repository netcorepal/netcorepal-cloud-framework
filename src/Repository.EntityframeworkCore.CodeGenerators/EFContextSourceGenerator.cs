using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace NetCorePal.Extensions.Repository.EntityframeworkCore.CodeGenerators
{
    [Generator]
    public class EFContextSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace",
                out var rootNamespace);
            if (rootNamespace == null)
            {
                return;
            }


            var compilation = context.Compilation;
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                if (syntaxTree.TryGetText(out var sourceText) &&
                    !sourceText.ToString().Contains("EFContext"))
                {
                    continue;
                }

                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                if (semanticModel == null)
                {
                    continue;
                }

                var typeDeclarationSyntaxs =
                    syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>();
                foreach (var tds in typeDeclarationSyntaxs)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(tds);
                    if (!(symbol is INamedTypeSymbol)) return;
                    INamedTypeSymbol namedTypeSymbol = (INamedTypeSymbol)symbol;


                    if (!namedTypeSymbol.IsAbstract && namedTypeSymbol?.BaseType?.Name == "EFContext")
                    {
                        List<INamedTypeSymbol> ids = GetAllIdTypes(context, namedTypeSymbol);
                        //ids.AddRange(GetIdNamedTypeSymbol(context.Compilation.Assembly));
                        GenerateValueConverters(context,namedTypeSymbol,ids,rootNamespace);

                        var refs = compilation.References.Where(p => p.Properties.Kind == MetadataImageKind.Assembly)
                            .ToList();
                        foreach (var r in refs)
                        {
                            var assembly = compilation.GetAssemblyOrModuleSymbol(r) as IAssemblySymbol;

                            if (assembly == null)
                            {
                                continue;
                            }

                            var nameprefix = compilation.AssemblyName?.Split('.').First();


                            if (assembly.Name.StartsWith(nameprefix))
                            {
                                ids.AddRange(GetIdNamedTypeSymbol(assembly));
                            }
                        }

                        if (ids.Count > 0)
                        {
                            Generate(context, namedTypeSymbol, ids, rootNamespace);
                        }
                    }
                }
            }
        }


        static List<INamedTypeSymbol> GetIdNamedTypeSymbol(IAssemblySymbol assemblySymbol)
        {
            List<INamedTypeSymbol> ids = new();
            var types = GetAllTypes(assemblySymbol);

            foreach (var type in types)
            {
                var stronglyTypedId = type.AllInterfaces.SingleOrDefault(t => t.Name == "IStronglyTypedId");
                if (!type.IsAbstract && type.AllInterfaces.Any(t => t.Name == "IStronglyTypedId"))
                {
                    ids.Add(type);
                }
            }

            return ids;
        }


        static List<INamedTypeSymbol> GetAllTypes(IAssemblySymbol assemblySymbol)
        {
            var types = new List<INamedTypeSymbol>();
            GetTypesInNamespace(assemblySymbol.GlobalNamespace, types);
            return types;
        }

        static void GetTypesInNamespace(INamespaceSymbol namespaceSymbol, List<INamedTypeSymbol> types)
        {
            // 获取当前命名空间中的类型
            types.AddRange(namespaceSymbol.GetTypeMembers());

            // 遍历所有子命名空间
            foreach (var subNamespaceSymbol in namespaceSymbol.GetNamespaceMembers())
            {
                GetTypesInNamespace(subNamespaceSymbol, types);
            }
        }


        private void Generate(GeneratorExecutionContext context, INamedTypeSymbol dbContextType,
            List<INamedTypeSymbol> ids, string rootNamespace)
        {
            var ns = dbContextType.ContainingNamespace.ToString();
            string className = dbContextType.Name;


            StringBuilder sb = new StringBuilder();

            foreach (var id in ids)
            {
                var idName = id.Name;
                sb.AppendLine(
                    $@"            configurationBuilder.Properties<{id.ContainingNamespace}.{id.Name}>().HaveConversion<{idName}ValueConverter>();");
            }

            string source = $@"// <auto-generated/>
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityframeworkCore;
using {rootNamespace}.ValueConverters;
namespace {ns}
{{
    public partial class {className} : EFContext
    {{
        private void ConfigureStronglyTypedIdValueConverter(ModelConfigurationBuilder configurationBuilder)
        {{
{sb}
        }}
    }}
}}
";
            context.AddSource($"{className}ValueConverter.g.cs", source);
        }

        void GenerateValueConverters(GeneratorExecutionContext context, INamedTypeSymbol dbContextType,
            List<INamedTypeSymbol> ids, string rootNamespace)
        {
            string source = $@"// <auto-generated/>
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
namespace {rootNamespace}.ValueConverters
{{
";
            foreach (var idType in ids)
            {
                var className = $"{idType.ContainingNamespace}.{idType.Name}";
                source += $@"   
    public partial class {idType.Name}ValueConverter : ValueConverter<{className}, {idType.AllInterfaces.First(t => t.Name == "IStronglyTypedId").TypeArguments[0].Name}>
    {{
        public  {idType.Name}ValueConverter() : base(p => p.Id, p => new {className}(p)) {{ }}
    }}
";
            }
            source += $@"
}}
";
            context.AddSource($"{dbContextType.Name}ValueConverters.g.cs", source);
        }


        List<INamedTypeSymbol> GetAllIdTypes(GeneratorExecutionContext context,
            INamedTypeSymbol dbContextType)
        {
            List<INamedTypeSymbol> ids = new List<INamedTypeSymbol>();
            List<INamedTypeSymbol> allType = new List<INamedTypeSymbol>();
            var members = dbContextType.GetMembers();
            foreach (var member in members)
            {
                if (member.Kind == SymbolKind.Property)
                {
                    var property = (IPropertySymbol)member;
                    if (property.Type.Name == "DbSet")
                    {
                        var type = property.Type as INamedTypeSymbol;
                        if (type == null) continue;
                        var typeArguments = type.TypeArguments;
                        if (typeArguments.Length == 1)
                        {
                            var idType = typeArguments[0] as INamedTypeSymbol;
                            if (idType == null) continue;
                            TryGetIdNamedTypeSymbol(idType, ids, allType);
                        }
                    }
                }
            }

            return ids;
        }

        void TryGetIdNamedTypeSymbol(INamedTypeSymbol namedTypeSymbol, List<INamedTypeSymbol> ids,
            List<INamedTypeSymbol> allType)
        {
            if (allType.Any(t =>
                    t.ContainingNamespace == namedTypeSymbol.ContainingNamespace && t.Name == namedTypeSymbol.Name))
            {
                return;
            }

            allType.Add(namedTypeSymbol);
            if (!namedTypeSymbol.IsAbstract && !namedTypeSymbol.IsGenericType &&
                namedTypeSymbol.AllInterfaces.Any(p => p.Name == "IStronglyTypedId") &&
                !ids.Any(t =>
                    t.ContainingNamespace == namedTypeSymbol.ContainingNamespace && t.Name == namedTypeSymbol.Name))
            {
                ids.Add(namedTypeSymbol);
            }

            var members = namedTypeSymbol.GetMembers();
            foreach (var member in members)
            {
                if (member.Kind == SymbolKind.Property)
                {
                    var property = (IPropertySymbol)member;
                    var type = property.Type as INamedTypeSymbol;
                    if (type == null) continue;
                    TryGetIdNamedTypeSymbol(type, ids, allType);
                }
            }
        }

        private void GenerateValueConverter(GeneratorExecutionContext context,
            INamedTypeSymbol dbContextType,
            string rootNamespace)
        {
            var ns = $"{rootNamespace}.ValueConverters";
            string className = dbContextType.Name;
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}