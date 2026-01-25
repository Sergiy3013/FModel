using AdonisUI.Controls;
using Microsoft.Win32;
using Serilog;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CUE4Parse;
using FModel.Framework;
using FModel.Services;
using FModel.Settings;
using FModel.ViewModels;
using Newtonsoft.Json;
using Serilog.Sinks.SystemConsole.Themes;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;

namespace FModel;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("winbrand.dll", CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern string BrandingFormatString(string format);

    protected override void OnStartup(StartupEventArgs e)
    {
        // Check for console mode flag in command-line arguments
        var args = Environment.GetCommandLineArgs();
        var isConsoleMode = args.Any(arg => arg.Equals("--console", StringComparison.OrdinalIgnoreCase) || 
                                            arg.Equals("-c", StringComparison.OrdinalIgnoreCase));
        
        if (isConsoleMode)
        {
            if (!AttachConsole(-1))
            {
                // Create a new console if we couldn't attach to parent
                AllocConsole();
            }
        }
#if DEBUG
        else
        {
            AttachConsole(-1);
        }
#endif
        base.OnStartup(e);

        try
        {
            UserSettings.Default = JsonConvert.DeserializeObject<UserSettings>(
                File.ReadAllText(UserSettings.FilePath), JsonNetSerializer.SerializerSettings);
        }
        catch
        {
            UserSettings.Default = new UserSettings();
        }

        var createMe = false;
        if (!Directory.Exists(UserSettings.Default.OutputDirectory))
        {
            var currentDir = Directory.GetCurrentDirectory();
            try
            {
                var outputDir = Directory.CreateDirectory(Path.Combine(currentDir, "Output"));
                using (File.Create(Path.Combine(outputDir.FullName, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
                {

                }

                UserSettings.Default.OutputDirectory = outputDir.FullName;
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new Exception("FModel cannot create the output directory where it is currently located. Please move FModel.exe to a different location.", exception);
            }
        }

        if (!Directory.Exists(UserSettings.Default.RawDataDirectory))
        {
            createMe = true;
            UserSettings.Default.RawDataDirectory = Path.Combine(UserSettings.Default.OutputDirectory, "Exports");
        }

        if (!Directory.Exists(UserSettings.Default.PropertiesDirectory))
        {
            createMe = true;
            UserSettings.Default.PropertiesDirectory = Path.Combine(UserSettings.Default.OutputDirectory, "Exports");
        }

        if (!Directory.Exists(UserSettings.Default.TextureDirectory))
        {
            createMe = true;
            UserSettings.Default.TextureDirectory = Path.Combine(UserSettings.Default.OutputDirectory, "Exports");
        }

        if (!Directory.Exists(UserSettings.Default.AudioDirectory))
        {
            createMe = true;
            UserSettings.Default.AudioDirectory = Path.Combine(UserSettings.Default.OutputDirectory, "Exports");
        }

        if (!Directory.Exists(UserSettings.Default.ModelDirectory))
        {
            createMe = true;
            UserSettings.Default.ModelDirectory = Path.Combine(UserSettings.Default.OutputDirectory, "Exports");
        }

        Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FModel"));
        Directory.CreateDirectory(Path.Combine(UserSettings.Default.OutputDirectory, "Backups"));
        if (createMe) Directory.CreateDirectory(Path.Combine(UserSettings.Default.OutputDirectory, "Exports"));
        Directory.CreateDirectory(Path.Combine(UserSettings.Default.OutputDirectory, "Logs"));
        Directory.CreateDirectory(Path.Combine(UserSettings.Default.OutputDirectory, ".data"));

        const string template = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Enriched}: {Message:lj}{NewLine}{Exception}";
        var logConfig = new LoggerConfiguration();
        
#if DEBUG
        logConfig = logConfig
            .Enrich.With<SourceEnricher>()
            .MinimumLevel.Verbose()
            .WriteTo.Console(outputTemplate: template, theme: AnsiConsoleTheme.Literate)
            .WriteTo.File(outputTemplate: template,
                path: Path.Combine(UserSettings.Default.OutputDirectory, "Logs", $"FModel-Debug-Log-{DateTime.Now:yyyy-MM-dd}.log"));
#else
        logConfig = logConfig
            .Enrich.With<CallerEnricher>()
            .WriteTo.File(outputTemplate: template,
                path: Path.Combine(UserSettings.Default.OutputDirectory, "Logs", $"FModel-Log-{DateTime.Now:yyyy-MM-dd}.log"));
        
        // Add console output in release mode if in console mode
        if (isConsoleMode)
        {
            logConfig = logConfig.WriteTo.Console(outputTemplate: template, theme: AnsiConsoleTheme.Literate);
        }
#endif
        
        Log.Logger = logConfig.CreateLogger();

        Log.Information("Version {Version} ({CommitId})", Constants.APP_VERSION, Constants.APP_COMMIT_ID);
        Log.Information("{OS}", GetOperatingSystemProductName());
        Log.Information("{RuntimeVer}", RuntimeInformation.FrameworkDescription);
        Log.Information("Culture {SysLang}", CultureInfo.CurrentCulture);
        
        // Handle console mode
        if (isConsoleMode)
        {
            Log.Information("Running in console mode");
            RunConsoleMode(args).GetAwaiter().GetResult();
            Log.Information("Console mode completed");
            Log.CloseAndFlush();
            UserSettings.Save();
            Environment.Exit(0);
        }
    }

    private void AppExit(object sender, ExitEventArgs e)
    {
        Log.Information("––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––");
        Log.CloseAndFlush();
        UserSettings.Save();
        Environment.Exit(0);
    }

    private async Task RunConsoleMode(string[] args)
    {
        try
        {
            Console.WriteLine("FModel Console Mode");
            Console.WriteLine("===================");
            Console.WriteLine();
            
            // Show help if requested or no arguments
            if (args.Length <= 1 || args.Any(a => a.Equals("--help", StringComparison.OrdinalIgnoreCase) || a.Equals("-h", StringComparison.OrdinalIgnoreCase)))
            {
                ShowConsoleHelp();
                return;
            }
            
            Console.WriteLine($"Initializing FModel...");
            Log.Information("Initializing console mode with {ArgCount} arguments", args.Length);
            
            // Initialize required services
            await ApplicationViewModel.InitOodle();
            await ApplicationViewModel.InitZlib();
            
            // Create minimal services (without WPF dependencies)
            var appViewModel = ApplicationService.ApplicationView;
            
            // Initialize CUE4Parse
            await appViewModel.CUE4Parse.Initialize();
            await appViewModel.AesManager.InitAes();
            await appViewModel.UpdateProvider(false);
            
            await Task.WhenAll(
                appViewModel.CUE4Parse.VerifyConsoleVariables(),
                appViewModel.CUE4Parse.VerifyOnDemandArchives(),
                appViewModel.CUE4Parse.InitMappings(),
                ApplicationViewModel.InitDetex(),
                ApplicationViewModel.InitVgmStream()
            );
            
            Console.WriteLine("Initialization complete.");
            Console.WriteLine();
            
            // Parse command - skip executable name and console flag
            var commandArgs = args.Skip(1) // Skip executable name
                .Where(a => !a.Equals("--console", StringComparison.OrdinalIgnoreCase) && 
                           !a.Equals("-c", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            
            var command = commandArgs.Length > 0 ? commandArgs[0] : null;
            
            switch (command?.ToLowerInvariant())
            {
                case "list":
                    ListAssets(commandArgs);
                    break;
                case "extract":
                    await ExtractAssets(commandArgs, appViewModel);
                    break;
                case "info":
                    ShowGameInfo(appViewModel);
                    break;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    Console.WriteLine("Use --help for usage information.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in console mode");
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    
    private void ShowConsoleHelp()
    {
        Console.WriteLine("Usage: FModel.exe --console <command> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  info                    - Display game and provider information");
        Console.WriteLine("  list [pattern]          - List available assets (optional: filter by pattern)");
        Console.WriteLine("  extract <path>          - Extract specified asset by path");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -c, --console           - Run in console mode");
        Console.WriteLine("  -h, --help              - Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  FModel.exe --console info");
        Console.WriteLine("  FModel.exe --console list");
        Console.WriteLine("  FModel.exe --console extract \"FortniteGame/Content/Items/Weapons/Rifle.uasset\"");
    }
    
    private void ListAssets(string[] commandArgs)
    {
        // commandArgs[0] is "list", commandArgs[1] (if exists) is the pattern
        var pattern = commandArgs.Length > 1 ? commandArgs[1] : "";
        var provider = ApplicationService.ApplicationView.CUE4Parse.Provider;
        
        Console.WriteLine($"Listing assets in {provider.GameDisplayName}...");
        Console.WriteLine();
        
        var files = provider.Files.Values.Where(f => 
            string.IsNullOrEmpty(pattern) || 
            f.Path.Contains(pattern, StringComparison.OrdinalIgnoreCase)
        ).Take(100).ToList();
        
        Console.WriteLine($"Found {files.Count} assets (showing first 100):");
        foreach (var file in files)
        {
            Console.WriteLine($"  {file.Path}");
        }
        
        if (files.Count == 100)
        {
            Console.WriteLine();
            Console.WriteLine("... (more files available, use pattern to filter)");
        }
    }
    
    private async Task ExtractAssets(string[] commandArgs, ApplicationViewModel appViewModel)
    {
        // commandArgs[0] is "extract", commandArgs[1] should be the asset path
        if (commandArgs.Length < 2)
        {
            Console.WriteLine("Error: Please specify asset path to extract");
            Console.WriteLine("Usage: FModel.exe --console extract <asset_path>");
            return;
        }
        
        var assetPath = commandArgs[1];
        var provider = appViewModel.CUE4Parse.Provider;
        
        Console.WriteLine($"Extracting asset: {assetPath}");
        
        if (!provider.Files.TryGetValue(assetPath, out var gameFile))
        {
            Console.WriteLine($"Error: Asset not found: {assetPath}");
            return;
        }
        
        try
        {
            var cts = new System.Threading.CancellationTokenSource();
            await Task.Run(() => 
            {
                appViewModel.CUE4Parse.Extract(cts.Token, gameFile);
            });
            
            Console.WriteLine($"Successfully extracted: {assetPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting asset: {ex.Message}");
            Log.Error(ex, "Failed to extract asset {AssetPath}", assetPath);
        }
    }
    
    private void ShowGameInfo(ApplicationViewModel appViewModel)
    {
        var provider = appViewModel.CUE4Parse.Provider;
        
        Console.WriteLine("Game Information:");
        Console.WriteLine($"  Game: {provider.GameDisplayName}");
        Console.WriteLine($"  Version: {UserSettings.Default.CurrentDir?.UeVersion}");
        Console.WriteLine($"  Files: {provider.Files.Count:N0}");
        Console.WriteLine($"  Output Directory: {UserSettings.Default.OutputDirectory}");
        Console.WriteLine();
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error("{Exception}", e.Exception);

        var messageBox = new MessageBoxModel
        {
            Text = $"An unhandled {e.Exception.GetBaseException().GetType()} occurred: {e.Exception.Message}",
            Caption = "Fatal Error",
            Icon = MessageBoxImage.Error,
            Buttons =
            [
                MessageBoxButtons.Custom("Reset Settings", EErrorKind.ResetSettings),
                MessageBoxButtons.Custom("Restart", EErrorKind.Restart),
                MessageBoxButtons.Custom("OK", EErrorKind.Ignore)
            ],
            IsSoundEnabled = false
        };

        MessageBox.Show(messageBox);
        if (messageBox.Result == MessageBoxResult.Custom && (EErrorKind) messageBox.ButtonPressed.Id != EErrorKind.Ignore)
        {
            if ((EErrorKind) messageBox.ButtonPressed.Id == EErrorKind.ResetSettings)
                UserSettings.Delete();

            ApplicationService.ApplicationView.Restart();
        }

        e.Handled = true;
    }

    private string GetOperatingSystemProductName()
    {
        var productName = string.Empty;
        try
        {
            productName = BrandingFormatString("%WINDOWS_LONG%");
        }
        catch
        {
            // ignored
        }

        if (string.IsNullOrEmpty(productName))
            productName = Environment.OSVersion.VersionString;

        return $"{productName} ({(Environment.Is64BitOperatingSystem ? "64" : "32")}-bit)";
    }

    public static string GetRegistryValue(string path, string name = null, RegistryHive root = RegistryHive.CurrentUser)
    {
        using var rk = RegistryKey.OpenBaseKey(root, RegistryView.Default).OpenSubKey(path);
        if (rk != null)
            return rk.GetValue(name, null) as string;
        return string.Empty;
    }
}
