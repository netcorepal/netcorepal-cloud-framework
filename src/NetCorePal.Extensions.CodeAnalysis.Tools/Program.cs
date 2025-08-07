using System.CommandLine;
using System.Reflection;
using System.Xml.Linq;
using NetCorePal.Extensions.CodeAnalysis;

namespace NetCorePal.Extensions.CodeAnalysis.Tools;

public class Program
{
    // 不再需要动态类型加载，直接使用静态引用

    private static CodeFlowAnalysisResult GetResultFromAssemblies(Assembly[] assemblies)
    {
        try
        {
            // 直接使用静态方法，不需要反射
            return CodeFlowAnalysisHelper.GetResultFromAssemblies(assemblies);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to analyze some assemblies: {ex.Message}");
            Console.WriteLine("Attempting to continue with a subset of assemblies...");
            
            // 尝试逐个分析程序集，跳过有问题的
            var workingAssemblies = new List<Assembly>();
            foreach (var assembly in assemblies)
            {
                try
                {
                    // 尝试获取该程序集的自定义属性
                    var attributes = assembly.GetCustomAttributes().ToList();
                    workingAssemblies.Add(assembly);
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"Skipping assembly {assembly.FullName}: {innerEx.Message}");
                }
            }
            
            if (workingAssemblies.Count > 0)
            {
                Console.WriteLine($"Continuing analysis with {workingAssemblies.Count} out of {assemblies.Length} assemblies.");
                return CodeFlowAnalysisHelper.GetResultFromAssemblies(workingAssemblies.ToArray());
            }
            else
            {
                Console.WriteLine("No assemblies could be analyzed. Returning empty result.");
                return new CodeFlowAnalysisResult();
            }
        }
    }

    private static string GenerateVisualizationHtml(CodeFlowAnalysisResult analysisResult, string title)
    {
        // 直接使用静态方法，不需要反射
        return VisualizationHtmlBuilder.GenerateVisualizationHtml(analysisResult, title);
    }

    public static async Task<int> Main(string[] args)
    {
        var rootCommand =
            new RootCommand(
                "NetCorePal Code Analysis Tool - Generate architecture visualization HTML files from .NET assemblies");

        var generateCommand = new Command("generate", "Generate HTML visualization from assemblies");

        var solutionOption = new Option<FileInfo>(
            name: "--solution",
            description: "Solution file to analyze (.sln)")
        {
            IsRequired = false
        };
        solutionOption.AddAlias("-s");

        var projectOption = new Option<FileInfo[]>(
            name: "--project",
            description: "Project files to analyze (.csproj)")
        {
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true
        };
        projectOption.AddAlias("-p");

        var assemblyOption = new Option<FileInfo[]>(
            name: "--assembly",
            description: "Assembly files to analyze (.dll)")
        {
            IsRequired = false,
            AllowMultipleArgumentsPerToken = true
        };
        assemblyOption.AddAlias("-a");

        var configurationOption = new Option<string>(
            name: "--configuration",
            description: "Build configuration")
        {
            IsRequired = false
        };
        configurationOption.AddAlias("-c");
        configurationOption.SetDefaultValue("Debug");

        var outputOption = new Option<FileInfo>(
            name: "--output",
            description: "Output HTML file path")
        {
            IsRequired = false
        };
        outputOption.AddAlias("-o");
        outputOption.SetDefaultValue(new FileInfo("code-analysis.html"));

        var titleOption = new Option<string>(
            name: "--title",
            description: "HTML page title")
        {
            IsRequired = false
        };
        titleOption.AddAlias("-t");
        titleOption.SetDefaultValue("Architecture Visualization");

        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Enable verbose output")
        {
            IsRequired = false
        };
        verboseOption.AddAlias("-v");

        var frameworkOption = new Option<string>(
            name: "--framework",
            description: "Target framework to analyze (when project has multiple frameworks)")
        {
            IsRequired = false
        };
        frameworkOption.AddAlias("-f");

        generateCommand.AddOption(solutionOption);
        generateCommand.AddOption(projectOption);
        generateCommand.AddOption(assemblyOption);
        generateCommand.AddOption(configurationOption);
        generateCommand.AddOption(outputOption);
        generateCommand.AddOption(titleOption);
        generateCommand.AddOption(verboseOption);
        generateCommand.AddOption(frameworkOption);

        generateCommand.SetHandler(
            async (solution, projects, assemblies, configuration, output, title, verbose, framework) =>
            {
                await GenerateVisualization(solution, projects, assemblies, configuration, output, title, verbose,
                    framework);
            }, solutionOption, projectOption, assemblyOption, configurationOption, outputOption, titleOption,
            verboseOption, frameworkOption);

        rootCommand.AddCommand(generateCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task GenerateVisualization(FileInfo? solutionFile, FileInfo[]? projectFiles,
        FileInfo[]? assemblyFiles,
        string configuration, FileInfo outputFile, string title, bool verbose, string? framework)
    {
        try
        {
            if (verbose)
            {
                Console.WriteLine($"NetCorePal Code Analysis Tool v{GetVersion()}");
                Console.WriteLine($"Output file: {outputFile.FullName}");
                Console.WriteLine($"Title: {title}");
                Console.WriteLine($"Configuration: {configuration}");
                Console.WriteLine();
            }

            // Determine what to analyze
            var assembliesToAnalyze = new List<Assembly>();

            if (assemblyFiles?.Length > 0)
            {
                // Direct assembly files specified
                if (verbose)
                    Console.WriteLine("Using specified assembly files:");

                LoadAssembliesFromFiles(assemblyFiles, assembliesToAnalyze, verbose);
            }
            else if (projectFiles?.Length > 0)
            {
                // Project files specified
                if (verbose)
                    Console.WriteLine("Building and analyzing specified projects:");

                await LoadAssembliesFromProjects(projectFiles, configuration, framework, assembliesToAnalyze, verbose);
            }
            else if (solutionFile != null)
            {
                // Solution file specified
                if (verbose)
                    Console.WriteLine($"Building and analyzing solution: {solutionFile.FullName}");

                await LoadAssembliesFromSolution(solutionFile, configuration, framework, assembliesToAnalyze, verbose);
            }
            else
            {
                // Auto-discover solution or projects in current directory
                if (verbose)
                    Console.WriteLine("Auto-discovering solution or projects in current directory...");

                await AutoDiscoverAndLoadAssemblies(configuration, framework, assembliesToAnalyze, verbose);
            }

            if (assembliesToAnalyze.Count == 0)
            {
                Console.Error.WriteLine(
                    "Error: No assemblies found to analyze. Please specify --solution, --project, or --assembly options.");
                Environment.Exit(1);
            }

            if (verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Analyzing assemblies...");
            }

            // Aggregate analysis results - 现在直接使用静态方法
            var analysisResult = GetResultFromAssemblies(assembliesToAnalyze.ToArray());

            if (verbose)
            {
                Console.WriteLine($"Analysis completed:");
                Console.WriteLine($"  Controllers: {analysisResult.Nodes.Count(n => n.Type == NodeType.Controller)}");
                Console.WriteLine($"  Commands: {analysisResult.Nodes.Count(n => n.Type == NodeType.Command)}");
                Console.WriteLine($"  Entities: {analysisResult.Nodes.Count(n => n.Type == NodeType.Aggregate)}");
                Console.WriteLine($"  Domain Events: {analysisResult.Nodes.Count(n => n.Type == NodeType.DomainEvent)}");
                Console.WriteLine($"  Integration Events: {analysisResult.Nodes.Count(n => n.Type == NodeType.IntegrationEvent)}");
                Console.WriteLine($"  Relationships: {analysisResult.Relationships.Count}");
                Console.WriteLine();
            }

            // Check if analysis result is empty and provide helpful guidance
            if (analysisResult.Nodes.Count(n => n.Type == NodeType.Controller) == 0 &&
                analysisResult.Nodes.Count(n => n.Type == NodeType.Command) == 0 &&
                analysisResult.Nodes.Count(n => n.Type == NodeType.Aggregate) == 0 &&
                analysisResult.Nodes.Count(n => n.Type == NodeType.DomainEvent) == 0 &&
                analysisResult.Nodes.Count(n => n.Type == NodeType.IntegrationEvent) == 0)
            {
                Console.WriteLine("⚠️  No code analysis results found in the assemblies.");
                Console.WriteLine();
                Console.WriteLine("This usually means one of the following:");
                Console.WriteLine("1. The project(s) don't use NetCorePal framework components");
                Console.WriteLine("2. Missing NetCorePal.Extensions.CodeAnalysis package reference");
                Console.WriteLine("3. The source generators haven't run during build");
                Console.WriteLine(
                    "4. If you are using .NET 9.0 or later, please ensure all related packages and source generators are compatible with your SDK version.");
                Console.WriteLine();
                Console.WriteLine("💡 To fix this, ensure your project includes:");
                Console.WriteLine("   <PackageReference Include=\"NetCorePal.Extensions.CodeAnalysis\" />");
                Console.WriteLine();
                Console.WriteLine("   Then rebuild your project and try again.");
                Console.WriteLine();
                Console.WriteLine("📖 For more information, visit:");
                Console.WriteLine(
                    "   https://netcorepal.github.io/netcorepal-cloud-framework/zh/code-analysis/code-analysis-tools/");
                Console.WriteLine();
                Console.WriteLine("⚡ Generating empty visualization anyway...");
                Console.WriteLine();
            }

            // Generate HTML
            if (verbose)
            {
                Console.WriteLine("Generating HTML visualization...");
            }

            var htmlContent = GenerateVisualizationHtml(analysisResult, title);

            // Ensure output directory exists
            if (outputFile.Directory != null && !outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
            }

            // Write HTML file
            await File.WriteAllTextAsync(outputFile.FullName, htmlContent);

            var hasAnalysisResults = analysisResult.Nodes.Count(n => n.Type == NodeType.Controller) > 0 ||
                                     analysisResult.Nodes.Count(n => n.Type == NodeType.Command) > 0 ||
                                     analysisResult.Nodes.Count(n => n.Type == NodeType.Aggregate) > 0 ||
                                     analysisResult.Nodes.Count(n => n.Type == NodeType.DomainEvent) > 0 ||
                                     analysisResult.Nodes.Count(n => n.Type == NodeType.IntegrationEvent) > 0;

            if (hasAnalysisResults)
            {
                Console.WriteLine($"✅ HTML visualization generated successfully: {outputFile.FullName}");
            }
            else
            {
                Console.WriteLine($"📄 Empty HTML visualization generated: {outputFile.FullName}");
                Console.WriteLine(
                    "   (Add NetCorePal.Extensions.CodeAnalysis package to your projects for meaningful results)");
            }

            if (verbose)
            {
                var fileInfo = new FileInfo(outputFile.FullName);
                Console.WriteLine($"File size: {fileInfo.Length:N0} bytes");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            if (verbose)
            {
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Environment.Exit(1);
        }
    }

    private static void LoadAssembliesFromFiles(FileInfo[] assemblyFiles, List<Assembly> assemblies, bool verbose)
    {
        foreach (var assemblyFile in assemblyFiles)
        {
            if (!assemblyFile.Exists)
            {
                Console.Error.WriteLine($"Error: Assembly file not found: {assemblyFile.FullName}");
                Environment.Exit(1);
            }

            try
            {
                // 检查是否已经加载过相同路径的程序集
                var normalizedPath = Path.GetFullPath(assemblyFile.FullName);
                var alreadyLoaded = assemblies.Any(a => 
                    !string.IsNullOrEmpty(a.Location) && 
                    Path.GetFullPath(a.Location).Equals(normalizedPath, StringComparison.OrdinalIgnoreCase));
                    
                if (alreadyLoaded)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"  Skipping already loaded assembly: {assemblyFile.Name}");
                    }
                }
                else
                {
                    var assembly = Assembly.LoadFrom(assemblyFile.FullName);
                    assemblies.Add(assembly);
                    if (verbose)
                    {
                        Console.WriteLine($"  Loaded assembly: {assembly.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading assembly {assemblyFile.FullName}: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }

    private static async Task LoadAssembliesFromProjects(FileInfo[] projectFiles, string configuration,
        string? framework, List<Assembly> assemblies, bool verbose)
    {
        foreach (var projectFile in projectFiles)
        {
            if (!projectFile.Exists)
            {
                Console.Error.WriteLine($"Error: Project file not found: {projectFile.FullName}");
                Environment.Exit(1);
            }

            await BuildAndLoadProject(projectFile.FullName, configuration, framework, assemblies, verbose);
        }
    }

    private static async Task LoadAssembliesFromSolution(FileInfo solutionFile, string configuration, string? framework,
        List<Assembly> assemblies, bool verbose)
    {
        if (!solutionFile.Exists)
        {
            Console.Error.WriteLine($"Error: Solution file not found: {solutionFile.FullName}");
            Environment.Exit(1);
        }

        await BuildAndLoadSolution(solutionFile.FullName, configuration, framework, assemblies, verbose);
    }

    private static async Task AutoDiscoverAndLoadAssemblies(string configuration, string? framework,
        List<Assembly> assemblies, bool verbose)
    {
        var currentDir = Directory.GetCurrentDirectory();

        // Look for solution files
        var solutionFiles = Directory.GetFiles(currentDir, "*.sln", SearchOption.TopDirectoryOnly);
        if (solutionFiles.Length > 0)
        {
            var solutionFile = solutionFiles[0]; // Use first solution found
            if (verbose)
                Console.WriteLine($"  Found solution: {Path.GetFileName(solutionFile)}");

            await BuildAndLoadSolution(solutionFile, configuration, framework, assemblies, verbose);
            return;
        }

        // Look for project files
        var projectFiles = Directory.GetFiles(currentDir, "*.csproj", SearchOption.TopDirectoryOnly);
        if (projectFiles.Length > 0)
        {
            if (verbose)
                Console.WriteLine($"  Found {projectFiles.Length} project(s):");

            foreach (var projectFile in projectFiles)
            {
                if (verbose)
                    Console.WriteLine($"    {Path.GetFileName(projectFile)}");
                await BuildAndLoadProject(projectFile, configuration, framework, assemblies, verbose);
            }

            return;
        }

        // Look for assemblies in bin directories
        var binDirs = Directory.GetDirectories(currentDir, "bin", SearchOption.AllDirectories);
        foreach (var binDir in binDirs)
        {
            var configDir = Path.Combine(binDir, configuration);
            if (Directory.Exists(configDir))
            {
                var assemblyFiles = Directory.GetFiles(configDir, "*.dll", SearchOption.AllDirectories)
                    .Where(f => !Path.GetFileName(f).StartsWith("System.") &&
                                !Path.GetFileName(f).StartsWith("Microsoft.") &&
                                !Path.GetFileName(f).StartsWith("Newtonsoft."))
                    .ToArray();

                if (assemblyFiles.Length > 0 && verbose)
                {
                    Console.WriteLine($"  Found assemblies in {configDir}:");
                }

                foreach (var assemblyFile in assemblyFiles)
                {
                    try
                    {
                        // 检查是否已经加载过相同路径的程序集
                        var normalizedPath = Path.GetFullPath(assemblyFile);
                        var alreadyLoaded = assemblies.Any(a => 
                            !string.IsNullOrEmpty(a.Location) && 
                            Path.GetFullPath(a.Location).Equals(normalizedPath, StringComparison.OrdinalIgnoreCase));
                            
                        if (alreadyLoaded)
                        {
                            if (verbose)
                            {
                                Console.WriteLine($"    Skipping already loaded: {Path.GetFileName(assemblyFile)}");
                            }
                        }
                        else
                        {
                            var assembly = Assembly.LoadFrom(assemblyFile);
                            assemblies.Add(assembly);
                            if (verbose)
                            {
                                Console.WriteLine($"    Loaded: {Path.GetFileName(assemblyFile)}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (verbose)
                        {
                            Console.WriteLine($"    Skipped {Path.GetFileName(assemblyFile)}: {ex.Message}");
                        }
                    }
                }
            }
        }
    }

    private static async Task BuildAndLoadSolution(string solutionPath, string configuration, string? framework,
        List<Assembly> assemblies, bool verbose)
    {
        try
        {
            // Build the solution
            var buildArgs = $"build \"{solutionPath}\" --configuration {configuration} --verbosity minimal";
            if (!string.IsNullOrEmpty(framework))
            {
                buildArgs += $" --framework {framework}";
            }

            if (verbose)
                Console.WriteLine($"  Building solution: dotnet {buildArgs}");

            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = buildArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var buildProcess = System.Diagnostics.Process.Start(processStartInfo);
            if (buildProcess == null)
            {
                Console.Error.WriteLine("Failed to start dotnet build process");
                Environment.Exit(1);
            }

            // 异步读取输出流以防止缓冲区满
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            var outputTask = Task.Run(async () =>
            {
                using var reader = buildProcess.StandardOutput;
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"    {line}");
                    }
                    outputBuilder.AppendLine(line);
                }
            });

            var errorTask = Task.Run(async () =>
            {
                using var reader = buildProcess.StandardError;
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    errorBuilder.AppendLine(line);
                }
            });

            // 设置超时时间（5分钟）
            var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            
            try
            {
                // 等待进程完成和输出读取完成
                await Task.WhenAll(
                    buildProcess.WaitForExitAsync(timeoutCts.Token),
                    outputTask,
                    errorTask
                );

                if (buildProcess.ExitCode != 0)
                {
                    var error = errorBuilder.ToString();
                    Console.Error.WriteLine($"Build failed with exit code {buildProcess.ExitCode}:");
                    Console.Error.WriteLine(error);
                    Environment.Exit(1);
                }

                if (verbose)
                {
                    Console.WriteLine("  Solution build completed successfully");
                }
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine("Build process timed out after 5 minutes");
                try
                {
                    if (!buildProcess.HasExited)
                    {
                        buildProcess.Kill(true);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to kill build process: {ex.Message}");
                }
                Environment.Exit(1);
            }

            // Find and load assemblies from the built solution
            var solutionDir = Path.GetDirectoryName(solutionPath)!;
            var projectPaths = GetProjectPathsFromSolution(solutionPath, solutionDir);
            
            if (verbose)
            {
                Console.WriteLine($"  Found {projectPaths.Count} projects in solution:");
                foreach (var projectPath in projectPaths)
                {
                    Console.WriteLine($"    {Path.GetFileName(projectPath)}");
                }
            }
            
            foreach (var projectPath in projectPaths)
            {
                var projectDir = Path.GetDirectoryName(projectPath)!;
                var projectName = Path.GetFileNameWithoutExtension(projectPath);

                // 使用项目文件加载程序集
                LoadAssemblyFromProject(projectDir, projectName, configuration, framework, assemblies, verbose);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error building solution: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task BuildAndLoadProject(string projectPath, string configuration, string? framework,
        List<Assembly> assemblies, bool verbose)
    {
        try
        {
            // Build the project
            var buildArgs = $"build \"{projectPath}\" --configuration {configuration} --verbosity minimal";
            if (!string.IsNullOrEmpty(framework))
            {
                buildArgs += $" --framework {framework}";
            }

            if (verbose)
                Console.WriteLine($"  Building project: dotnet {buildArgs}");

            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = buildArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var buildProcess = System.Diagnostics.Process.Start(processStartInfo);
            if (buildProcess == null)
            {
                Console.Error.WriteLine($"Failed to start dotnet build process for {Path.GetFileName(projectPath)}");
                return;
            }

            // 异步读取输出流以防止缓冲区满
            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

            var outputTask = Task.Run(async () =>
            {
                using var reader = buildProcess.StandardOutput;
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"    {line}");
                    }
                    outputBuilder.AppendLine(line);
                }
            });

            var errorTask = Task.Run(async () =>
            {
                using var reader = buildProcess.StandardError;
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    errorBuilder.AppendLine(line);
                }
            });

            // 设置超时时间（3分钟，单个项目构建时间较短）
            var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(3));
            
            try
            {
                // 等待进程完成和输出读取完成
                await Task.WhenAll(
                    buildProcess.WaitForExitAsync(timeoutCts.Token),
                    outputTask,
                    errorTask
                );

                if (buildProcess.ExitCode != 0)
                {
                    var error = errorBuilder.ToString();
                    Console.Error.WriteLine($"Build failed for {Path.GetFileName(projectPath)} with exit code {buildProcess.ExitCode}:");
                    Console.Error.WriteLine(error);
                    return; // Continue with other projects
                }

                if (verbose)
                {
                    Console.WriteLine($"  Project {Path.GetFileName(projectPath)} build completed successfully");
                }
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine($"Build process for {Path.GetFileName(projectPath)} timed out after 3 minutes");
                try
                {
                    if (!buildProcess.HasExited)
                    {
                        buildProcess.Kill(true);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to kill build process for {Path.GetFileName(projectPath)}: {ex.Message}");
                }
                return;
            }

            // Find and load assembly from the built project
            var projectDir = Path.GetDirectoryName(projectPath)!;
            var projectName = Path.GetFileNameWithoutExtension(projectPath);

            LoadAssemblyFromProject(projectDir, projectName, configuration, framework, assemblies, verbose);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error building project {Path.GetFileName(projectPath)}: {ex.Message}");
        }
    }

    private static List<string> GetProjectPathsFromSolution(string solutionPath, string solutionDir)
    {
        var projectPaths = new List<string>();
        
        try
        {
            if (solutionPath.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase))
            {
                // 解析新的 XML 格式解决方案文件
                var doc = XDocument.Load(solutionPath);
                var projectElements = doc.Descendants("Project");
                
                foreach (var projectElement in projectElements)
                {
                    var pathAttr = projectElement.Attribute("Path")?.Value;
                    if (!string.IsNullOrEmpty(pathAttr))
                    {
                        // 将相对路径转换为绝对路径
                        var absolutePath = Path.IsPathRooted(pathAttr) 
                            ? pathAttr 
                            : Path.GetFullPath(Path.Combine(solutionDir, pathAttr.Replace('\\', Path.DirectorySeparatorChar)));
                        
                        if (File.Exists(absolutePath))
                        {
                            projectPaths.Add(absolutePath);
                        }
                    }
                }
            }
            else if (solutionPath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
            {
                // 解析传统的 .sln 格式文件
                var lines = File.ReadAllLines(solutionPath);
                
                foreach (var line in lines)
                {
                    if (line.StartsWith("Project("))
                    {
                        // 解析项目行: Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "ProjectName", "src\ProjectName\ProjectName.csproj", "{GUID}"
                        var parts = line.Split('=')[1].Split(',');
                        if (parts.Length >= 2)
                        {
                            var projectPath = parts[1].Trim().Trim('"');
                            var absolutePath = Path.IsPathRooted(projectPath) 
                                ? projectPath 
                                : Path.GetFullPath(Path.Combine(solutionDir, projectPath.Replace('\\', Path.DirectorySeparatorChar)));
                            
                            if (File.Exists(absolutePath) && absolutePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                            {
                                projectPaths.Add(absolutePath);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Warning: Failed to parse solution file {solutionPath}: {ex.Message}");
            Console.Error.WriteLine("Falling back to directory scanning...");
            
            // 回退到目录扫描
            var projectFiles = Directory.GetFiles(solutionDir, "*.csproj", SearchOption.AllDirectories);
            projectPaths.AddRange(projectFiles);
        }
        
        return projectPaths;
    }

    private static void LoadAssembliesFromDirectory(string directory, string configuration, string? framework, List<Assembly> assemblies, bool verbose)
    {
        var projectFiles = Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories);
        foreach (var projectFile in projectFiles)
        {
            var projectDir = Path.GetDirectoryName(projectFile)!;
            var projectName = Path.GetFileNameWithoutExtension(projectFile);

            // 使用项目文件加载程序集
            LoadAssemblyFromProject(projectDir, projectName, configuration, framework, assemblies, verbose);
        }
    }

    private static void LoadAssemblyFromProject(string projectDir, string projectName, string configuration,
        string? framework, List<Assembly> assemblies, bool verbose)
    {
        var binDir = Path.Combine(projectDir, "bin", configuration);
        if (!Directory.Exists(binDir))
            return;

        var projectFile = Path.Combine(projectDir, projectName + ".csproj");

        // 从项目文件中读取 target frameworks
        var targetFrameworks = GetTargetFrameworksFromProject(projectFile);

        if (targetFrameworks.Count == 0)
        {
            if (verbose)
            {
                Console.WriteLine($"    No target frameworks found in {projectName}.csproj");
            }

            return;
        }

        if (verbose)
        {
            Console.WriteLine($"    Target frameworks in {projectName}: {string.Join(", ", targetFrameworks)}");
        }

        // 如果有多个目标框架，选择一个并给出告警
        string selectedFramework;
        if (!string.IsNullOrEmpty(framework))
        {
            // 用户指定了框架，验证它是否在目标框架列表中
            if (targetFrameworks.Contains(framework))
            {
                selectedFramework = framework;
                if (verbose)
                {
                    Console.WriteLine($"    Using specified framework: {selectedFramework}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️  Specified framework '{framework}' not found in project {projectName}");
                Console.WriteLine($"    Available frameworks: {string.Join(", ", targetFrameworks)}");
                Console.WriteLine($"    Falling back to automatic selection");
                selectedFramework = SelectBestFramework(targetFrameworks);
            }
        }
        else if (targetFrameworks.Count > 1)
        {
            // 优先选择最新的 .NET 版本
            selectedFramework = SelectBestFramework(targetFrameworks);
            Console.WriteLine(
                $"⚠️  Project {projectName} targets multiple frameworks: {string.Join(", ", targetFrameworks)}");
            Console.WriteLine($"    Selected framework: {selectedFramework}");
            Console.WriteLine($"    To analyze a specific framework, use --framework option");
        }
        else
        {
            selectedFramework = targetFrameworks[0];
        }

        // 加载项目依赖
        var projectDependencies = GetProjectDependencies(projectFile);
        if (projectDependencies.Count > 0 && verbose)
        {
            Console.WriteLine($"    Found {projectDependencies.Count} project dependencies:");
            foreach (var dep in projectDependencies)
            {
                Console.WriteLine($"      {dep}");
            }
        }

        // 递归加载依赖的项目程序集
        var loadedProjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        LoadProjectWithDependencies(projectDir, projectName, selectedFramework, configuration, projectDependencies,
            assemblies, loadedProjects, verbose);
    }

    private static void LoadProjectWithDependencies(string projectDir, string projectName, string framework,
        string configuration,
        List<string> projectDependencies, List<Assembly> assemblies, HashSet<string> loadedProjects, bool verbose)
    {
        // 防止循环依赖
        var projectKey = $"{projectName}:{framework}";
        if (loadedProjects.Contains(projectKey))
        {
            if (verbose)
            {
                Console.WriteLine($"    Skipping already loaded project: {projectName} ({framework})");
            }

            return;
        }

        loadedProjects.Add(projectKey);

        // 加载当前项目的程序集
        var binDir = Path.Combine(projectDir, "bin", configuration);
        var fwDir = Path.Combine(binDir, framework);
        if (Directory.Exists(fwDir))
        {
            var assemblyPath = Path.Combine(fwDir, projectName + ".dll");
            if (File.Exists(assemblyPath))
            {
                try
                {
                    // 检查是否已经加载过相同路径的程序集
                    var normalizedPath = Path.GetFullPath(assemblyPath);
                    var alreadyLoaded = assemblies.Any(a => 
                        !string.IsNullOrEmpty(a.Location) && 
                        Path.GetFullPath(a.Location).Equals(normalizedPath, StringComparison.OrdinalIgnoreCase));
                        
                    if (alreadyLoaded)
                    {
                        if (verbose)
                        {
                            Console.WriteLine($"    Skipping already loaded assembly: {projectName}.dll ({framework})");
                        }
                    }
                    else
                    {
                        var assembly = Assembly.LoadFrom(assemblyPath);
                        assemblies.Add(assembly);
                        if (verbose)
                        {
                            Console.WriteLine($"    Loaded: {projectName}.dll ({framework})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (verbose)
                    {
                        Console.WriteLine($"    Failed to load {projectName}.dll ({framework}): {ex.Message}");
                    }
                }
            }
            else if (verbose)
            {
                Console.WriteLine($"    Assembly not found: {assemblyPath}");
            }
        }

        // 递归加载依赖项目
        foreach (var depPath in projectDependencies)
        {
            // 标准化路径分隔符（Windows使用\，Unix使用/）
            var normalizedDepPath = depPath.Replace('\\', Path.DirectorySeparatorChar);

            // 正确解析相对路径
            var depProjectFile = Path.IsPathRooted(normalizedDepPath)
                ? normalizedDepPath
                : Path.GetFullPath(Path.Combine(projectDir, normalizedDepPath));

            // 标准化路径
            depProjectFile = Path.GetFullPath(depProjectFile);

            if (File.Exists(depProjectFile))
            {
                var depProjectDir = Path.GetDirectoryName(depProjectFile)!;
                var depProjectName = Path.GetFileNameWithoutExtension(depProjectFile);

                if (verbose)
                {
                    Console.WriteLine($"      Processing dependency: {depProjectName} at {depProjectFile}");
                }

                // 获取依赖项目的目标框架
                var depTargetFrameworks = GetTargetFrameworksFromProject(depProjectFile);

                // 选择与当前项目兼容的框架
                var compatibleFramework = SelectCompatibleFramework(framework, depTargetFrameworks);

                if (!string.IsNullOrEmpty(compatibleFramework))
                {
                    // 递归获取依赖项目的依赖
                    var depDependencies = GetProjectDependencies(depProjectFile);
                    LoadProjectWithDependencies(depProjectDir, depProjectName, compatibleFramework, configuration,
                        depDependencies, assemblies, loadedProjects, verbose);
                }
                else if (verbose)
                {
                    Console.WriteLine($"      No compatible framework found for dependency {depProjectName}");
                    Console.WriteLine(
                        $"        Required: {framework}, Available: {string.Join(", ", depTargetFrameworks)}");
                }
            }
            else
            {
                if (verbose)
                {
                    Console.WriteLine($"      Dependency project file not found: {depProjectFile}");
                    Console.WriteLine($"        Original path: {depPath}");
                    Console.WriteLine($"        Normalized path: {normalizedDepPath}");
                    Console.WriteLine($"        Resolved from: {projectDir}");

                    // 尝试查找可能的路径
                    var alternativePaths = new[]
                    {
                        Path.Combine(projectDir, normalizedDepPath),
                        Path.Combine(Path.GetDirectoryName(projectDir)!, Path.GetFileName(normalizedDepPath)),
                        Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(projectDir)!)!,
                            Path.GetFileName(normalizedDepPath))
                    };

                    Console.WriteLine($"        Attempted paths:");
                    foreach (var altPath in alternativePaths)
                    {
                        var fullAltPath = Path.GetFullPath(altPath);
                        var exists = File.Exists(fullAltPath);
                        Console.WriteLine($"          {fullAltPath} - {(exists ? "EXISTS" : "NOT FOUND")}");
                    }
                }
            }
        }
    }

    private static List<string> GetProjectDependencies(string projectFilePath)
    {
        var dependencies = new List<string>();

        if (!File.Exists(projectFilePath))
        {
            return dependencies;
        }

        try
        {
            var doc = XDocument.Load(projectFilePath);

            // 查找 ProjectReference 元素
            var projectReferences = doc.Descendants("ProjectReference");

            foreach (var reference in projectReferences)
            {
                var includePath = reference.Attribute("Include")?.Value?.Trim();
                if (!string.IsNullOrEmpty(includePath))
                {
                    dependencies.Add(includePath);
                }
            }
        }
        catch (Exception)
        {
            // 如果解析失败，返回空列表
        }

        return dependencies;
    }

    private static string? SelectCompatibleFramework(string requiredFramework, List<string> availableFrameworks)
    {
        // 只考虑支持的框架（.NET 8.0及以上）
        var supportedFrameworks = availableFrameworks.Where(IsSupported).ToList();

        // 首先尝试找到完全匹配的框架
        if (supportedFrameworks.Contains(requiredFramework))
        {
            return requiredFramework;
        }

        // 提取主要版本信息进行兼容性检查
        var requiredVersion = ExtractFrameworkVersion(requiredFramework);
        var requiredType = GetFrameworkType(requiredFramework);

        // 寻找兼容的框架（只在支持的框架中查找）
        var compatibleFrameworks = supportedFrameworks
            .Where(fw =>
            {
                var fwType = GetFrameworkType(fw);
                var fwVersion = ExtractFrameworkVersion(fw);

                // 相同类型的框架 - 只支持现代.NET
                if (fwType == "net" && requiredType == "net")
                {
                    return fwVersion <= requiredVersion && fwVersion >= 8.0; // 依赖可以使用更低或相等的版本，但必须是.NET 8+
                }

                return false;
            })
            .OrderByDescending(fw => ExtractFrameworkVersion(fw))
            .FirstOrDefault();

        return compatibleFrameworks;
    }

    private static string GetFrameworkType(string framework)
    {
        if (framework.StartsWith("net") && char.IsDigit(framework[3]))
        {
            return "net";
        }

        if (framework.StartsWith("netcoreapp"))
        {
            return "netcoreapp";
        }

        if (framework.StartsWith("netstandard"))
        {
            return "netstandard";
        }

        return "unknown";
    }

    private static double ExtractFrameworkVersion(string framework)
    {
        if (framework.StartsWith("net") && char.IsDigit(framework[3]))
        {
            var versionPart = framework.Substring(3).Split('-')[0];
            if (double.TryParse(versionPart, out var version))
            {
                return version;
            }
        }
        else if (framework.StartsWith("netcoreapp"))
        {
            var versionPart = framework.Substring(10);
            if (double.TryParse(versionPart, out var version))
            {
                return version;
            }
        }
        else if (framework.StartsWith("netstandard"))
        {
            var versionPart = framework.Substring(11);
            if (double.TryParse(versionPart, out var version))
            {
                return version;
            }
        }

        return 0.0;
    }

    private static string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                      ?? assembly.GetName().Version?.ToString()
                      ?? "1.0.0";
        return version;
    }

    private static List<string> GetTargetFrameworksFromProject(string projectFilePath)
    {
        var frameworks = new List<string>();

        if (!File.Exists(projectFilePath))
        {
            return frameworks;
        }

        try
        {
            var doc = XDocument.Load(projectFilePath);

            // 查找 TargetFramework 或 TargetFrameworks 元素
            var targetFrameworkElements = doc.Descendants("TargetFramework");
            var targetFrameworksElements = doc.Descendants("TargetFrameworks");

            // 处理单个 target framework
            foreach (var element in targetFrameworkElements)
            {
                var value = element.Value?.Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    frameworks.Add(value);
                }
            }

            // 处理多个 target frameworks (分号分隔)
            foreach (var element in targetFrameworksElements)
            {
                var value = element.Value?.Trim();
                if (!string.IsNullOrEmpty(value))
                {
                    var tfms = value.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim())
                        .Where(f => !string.IsNullOrEmpty(f));
                    frameworks.AddRange(tfms);
                }
            }
        }
        catch (Exception)
        {
            // 如果解析失败，返回空列表
        }

        return frameworks.Distinct().ToList();
    }

    private static string SelectBestFramework(List<string> targetFrameworks)
    {
        // 框架优先级排序（最新的排在前面，只支持.NET 8及以上版本）
        var frameworkPriority = new Dictionary<string, int>
        {
            // .NET 9.0+
            { "net9.0", 900 },
            { "net9.0-windows", 900 },
            { "net9.0-macos", 900 },
            { "net9.0-linux", 900 },
            { "net9.0-android", 900 },
            { "net9.0-ios", 900 },

            // .NET 8.0
            { "net8.0", 800 },
            { "net8.0-windows", 800 },
            { "net8.0-macos", 800 },
            { "net8.0-linux", 800 },
            { "net8.0-android", 800 },
            { "net8.0-ios", 800 }
        };

        // 找到优先级最高的框架，并过滤掉低于.NET 8.0的框架
        var bestFramework = targetFrameworks
            .Where(fw => IsSupported(fw)) // 只保留支持的框架
            .OrderByDescending(fw =>
            {
                if (frameworkPriority.TryGetValue(fw, out var priority))
                    return priority;

                // 对于未知的框架，尝试从版本号推断优先级
                if (fw.StartsWith("net") && char.IsDigit(fw[3]))
                {
                    // 提取版本号并转换为数字
                    var versionPart = fw.Substring(3).Split('-')[0];
                    if (double.TryParse(versionPart, out var version))
                    {
                        return (int)(version * 100);
                    }
                }

                return 0; // 未知框架的默认优先级
            })
            .FirstOrDefault();

        if (bestFramework == null)
        {
            throw new InvalidOperationException(
                $"No supported target framework found. Only .NET 8.0 and above are supported. Available frameworks: {string.Join(", ", targetFrameworks)}");
        }

        return bestFramework;
    }

    private static bool IsSupported(string framework)
    {
        // 支持 .NET 8.0 及以上版本 - 现代.NET的版本号格式
        if (framework.StartsWith("net") && char.IsDigit(framework[3]))
        {
            var versionPart = framework.Substring(3).Split('-')[0];
            if (double.TryParse(versionPart, out var version))
            {
                // 只有版本号 >= 5.0 的才是现代.NET，5.0以下的都是.NET Framework
                // .NET Framework 使用 net48, net472 等格式，版本号是4.8, 4.72等
                // 现代.NET 使用 net5.0, net6.0, net8.0 等格式
                return version >= 8.0 && version < 48.0; // 排除.NET Framework的高版本号
            }
        }

        // 不支持其他框架类型（netcoreapp, netstandard, netframework等）
        return false;
    }
}