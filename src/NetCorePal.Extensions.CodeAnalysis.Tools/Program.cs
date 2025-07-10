using System.CommandLine;
using System.Reflection;
using NetCorePal.Extensions.CodeAnalysis;

namespace NetCorePal.Extensions.CodeAnalysis.Tools;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("NetCorePal Code Analysis Tool - Generate architecture visualization HTML files from .NET assemblies");

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

        generateCommand.AddOption(solutionOption);
        generateCommand.AddOption(projectOption);
        generateCommand.AddOption(assemblyOption);
        generateCommand.AddOption(configurationOption);
        generateCommand.AddOption(outputOption);
        generateCommand.AddOption(titleOption);
        generateCommand.AddOption(verboseOption);

        generateCommand.SetHandler(async (solution, projects, assemblies, configuration, output, title, verbose) =>
        {
            await GenerateVisualization(solution, projects, assemblies, configuration, output, title, verbose);
        }, solutionOption, projectOption, assemblyOption, configurationOption, outputOption, titleOption, verboseOption);

        rootCommand.AddCommand(generateCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task GenerateVisualization(FileInfo? solutionFile, FileInfo[]? projectFiles, FileInfo[]? assemblyFiles, 
        string configuration, FileInfo outputFile, string title, bool verbose)
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
                
                await LoadAssembliesFromProjects(projectFiles, configuration, null, assembliesToAnalyze, verbose);
            }
            else if (solutionFile != null)
            {
                // Solution file specified
                if (verbose)
                    Console.WriteLine($"Building and analyzing solution: {solutionFile.FullName}");
                
                await LoadAssembliesFromSolution(solutionFile, configuration, null, assembliesToAnalyze, verbose);
            }
            else
            {
                // Auto-discover solution or projects in current directory
                if (verbose)
                    Console.WriteLine("Auto-discovering solution or projects in current directory...");
                
                await AutoDiscoverAndLoadAssemblies(configuration, null, assembliesToAnalyze, verbose);
            }

            if (assembliesToAnalyze.Count == 0)
            {
                Console.Error.WriteLine("Error: No assemblies found to analyze. Please specify --solution, --project, or --assembly options.");
                Environment.Exit(1);
            }

            if (verbose)
            {
                Console.WriteLine();
                Console.WriteLine("Analyzing assemblies...");
            }

            // Aggregate analysis results
            var analysisResult = AnalysisResultAggregator.Aggregate(assembliesToAnalyze.ToArray());

            if (verbose)
            {
                Console.WriteLine($"Analysis completed:");
                Console.WriteLine($"  Controllers: {analysisResult.Controllers.Count}");
                Console.WriteLine($"  Commands: {analysisResult.Commands.Count}");
                Console.WriteLine($"  Entities: {analysisResult.Entities.Count}");
                Console.WriteLine($"  Domain Events: {analysisResult.DomainEvents.Count}");
                Console.WriteLine($"  Integration Events: {analysisResult.IntegrationEvents.Count}");
                Console.WriteLine($"  Relationships: {analysisResult.Relationships.Count}");
                Console.WriteLine();
            }

            // Check if analysis result is empty and provide helpful guidance
            if (analysisResult.Controllers.Count == 0 && 
                analysisResult.Commands.Count == 0 && 
                analysisResult.Entities.Count == 0 && 
                analysisResult.DomainEvents.Count == 0 && 
                analysisResult.IntegrationEvents.Count == 0)
            {
                Console.WriteLine("⚠️  No code analysis results found in the assemblies.");
                Console.WriteLine();
                Console.WriteLine("This usually means one of the following:");
                Console.WriteLine("1. The project(s) don't use NetCorePal framework components");
                Console.WriteLine("2. Missing NetCorePal.Extensions.CodeAnalysis package reference");
                Console.WriteLine("3. The source generators haven't run during build");
                Console.WriteLine("4. If you are using .NET 9.0 or later, please ensure all related packages and source generators are compatible with your SDK version.");
                Console.WriteLine();
                Console.WriteLine("💡 To fix this, ensure your project includes:");
                Console.WriteLine("   <PackageReference Include=\"NetCorePal.Extensions.CodeAnalysis\" />");
                Console.WriteLine();
                Console.WriteLine("   Then rebuild your project and try again.");
                Console.WriteLine();
                Console.WriteLine("📖 For more information, visit:");
                Console.WriteLine("   https://netcorepal.github.io/netcorepal-cloud-framework/zh/code-analysis/code-analysis-tools/");
                Console.WriteLine();
                Console.WriteLine("⚡ Generating empty visualization anyway...");
                Console.WriteLine();
            }

            // Generate HTML
            if (verbose)
            {
                Console.WriteLine("Generating HTML visualization...");
            }

            var htmlContent = MermaidVisualizer.GenerateVisualizationHtml(analysisResult, title);

            // Ensure output directory exists
            if (outputFile.Directory != null && !outputFile.Directory.Exists)
            {
                outputFile.Directory.Create();
            }

            // Write HTML file
            await File.WriteAllTextAsync(outputFile.FullName, htmlContent);

            var hasAnalysisResults = analysisResult.Controllers.Count > 0 || 
                                   analysisResult.Commands.Count > 0 || 
                                   analysisResult.Entities.Count > 0 || 
                                   analysisResult.DomainEvents.Count > 0 || 
                                   analysisResult.IntegrationEvents.Count > 0;

            if (hasAnalysisResults)
            {
                Console.WriteLine($"✅ HTML visualization generated successfully: {outputFile.FullName}");
            }
            else
            {
                Console.WriteLine($"📄 Empty HTML visualization generated: {outputFile.FullName}");
                Console.WriteLine("   (Add NetCorePal.Extensions.CodeAnalysis package to your projects for meaningful results)");
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
                var assembly = Assembly.LoadFrom(assemblyFile.FullName);
                assemblies.Add(assembly);
                if (verbose)
                {
                    Console.WriteLine($"  Loaded assembly: {assembly.FullName}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error loading assembly {assemblyFile.FullName}: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }

    private static async Task LoadAssembliesFromProjects(FileInfo[] projectFiles, string configuration, string? framework, List<Assembly> assemblies, bool verbose)
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

    private static async Task LoadAssembliesFromSolution(FileInfo solutionFile, string configuration, string? framework, List<Assembly> assemblies, bool verbose)
    {
        if (!solutionFile.Exists)
        {
            Console.Error.WriteLine($"Error: Solution file not found: {solutionFile.FullName}");
            Environment.Exit(1);
        }

        await BuildAndLoadSolution(solutionFile.FullName, configuration, framework, assemblies, verbose);
    }

    private static async Task AutoDiscoverAndLoadAssemblies(string configuration, string? framework, List<Assembly> assemblies, bool verbose)
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
                        var assembly = Assembly.LoadFrom(assemblyFile);
                        assemblies.Add(assembly);
                        if (verbose)
                        {
                            Console.WriteLine($"    Loaded: {Path.GetFileName(assemblyFile)}");
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

    private static async Task BuildAndLoadSolution(string solutionPath, string configuration, string? framework, List<Assembly> assemblies, bool verbose)
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

            var buildProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = buildArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (buildProcess != null)
            {
                await buildProcess.WaitForExitAsync();
                
                if (buildProcess.ExitCode != 0)
                {
                    var error = await buildProcess.StandardError.ReadToEndAsync();
                    Console.Error.WriteLine($"Build failed: {error}");
                    Environment.Exit(1);
                }
            }

            // Find and load assemblies from the built solution
            var solutionDir = Path.GetDirectoryName(solutionPath)!;
            LoadAssembliesFromDirectory(solutionDir, configuration, framework, assemblies, verbose);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error building solution: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task BuildAndLoadProject(string projectPath, string configuration, string? framework, List<Assembly> assemblies, bool verbose)
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

            var buildProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = buildArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (buildProcess != null)
            {
                await buildProcess.WaitForExitAsync();
                
                if (buildProcess.ExitCode != 0)
                {
                    var error = await buildProcess.StandardError.ReadToEndAsync();
                    Console.Error.WriteLine($"Build failed for {Path.GetFileName(projectPath)}: {error}");
                    return; // Continue with other projects
                }
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

    private static void LoadAssembliesFromDirectory(string directory, string configuration, string? framework, List<Assembly> assemblies, bool verbose)
    {
        var projectFiles = Directory.GetFiles(directory, "*.csproj", SearchOption.AllDirectories);
        foreach (var projectFile in projectFiles)
        {
            var projectDir = Path.GetDirectoryName(projectFile)!;
            var projectName = Path.GetFileNameWithoutExtension(projectFile);
            var binDir = Path.Combine(projectDir, "bin", configuration);
            if (!Directory.Exists(binDir))
                continue;

            // 查找所有 target framework 目录
            var frameworkDirs = Directory.GetDirectories(binDir).Where(d => 
                Path.GetFileName(d).StartsWith("net") || 
                Path.GetFileName(d).StartsWith("netstandard") ||
                Path.GetFileName(d).StartsWith("netcoreapp")).ToArray();

            foreach (var fwDir in frameworkDirs)
            {
                var assemblyPath = Path.Combine(fwDir, projectName + ".dll");
                if (File.Exists(assemblyPath))
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(assemblyPath);
                        assemblies.Add(assembly);
                        if (verbose)
                        {
                            Console.WriteLine($"    Loaded: {projectName}.dll ({fwDir})");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (verbose)
                        {
                            Console.WriteLine($"    Skipped {projectName}.dll ({fwDir}): {ex.Message}");
                        }
                    }
                }
            }
        }
    }

    private static void LoadAssemblyFromProject(string projectDir, string projectName, string configuration, string? framework, List<Assembly> assemblies, bool verbose)
    {
        var binDir = Path.Combine(projectDir, "bin", configuration);
        if (!Directory.Exists(binDir))
            return;

        // 收集所有本地源码 csproj
        var solutionRoot = Directory.GetParent(projectDir)?.FullName ?? projectDir;
        var allCsproj = Directory.GetFiles(solutionRoot, "*.csproj", SearchOption.AllDirectories)
            .Select(f => new { Name = Path.GetFileNameWithoutExtension(f), Dir = Path.GetDirectoryName(f) })
            .ToList();

        // 查找所有 target framework 目录
        var frameworkDirs = Directory.GetDirectories(binDir).Where(d =>
            Path.GetFileName(d).StartsWith("net") ||
            Path.GetFileName(d).StartsWith("netstandard") ||
            Path.GetFileName(d).StartsWith("netcoreapp")).ToArray();

        foreach (var fwDir in frameworkDirs)
        {
            foreach (var proj in allCsproj)
            {
                var dllPath = Path.Combine(fwDir, proj.Name + ".dll");
                if (File.Exists(dllPath))
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(dllPath);
                        assemblies.Add(assembly);
                        if (verbose)
                        {
                            Console.WriteLine($"    Loaded: {proj.Name}.dll ({fwDir})");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (verbose)
                        {
                            Console.WriteLine($"    Failed to load {proj.Name}.dll ({fwDir}): {ex.Message}");
                        }
                    }
                }
            }
        }
    }

    private static string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                     ?? assembly.GetName().Version?.ToString()
                     ?? "1.0.0";
        return version;
    }
}
