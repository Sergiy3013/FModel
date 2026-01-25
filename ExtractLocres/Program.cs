using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Versions;
using CUE4Parse.Compression;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text;

// Налаштування UTF-8 кодування для консолі
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// Налаштування опцій командного рядка
var pakFileOption = new Option<string>(
    name: "--pak",
    description: "Шлях до pak файлу для екстракції")
{ IsRequired = false };
pakFileOption.AddAlias("-p");

var scanDirOption = new Option<string>(
    name: "--scan",
    description: "Папка для сканування всіх pak файлів у підпапках")
{ IsRequired = false };
scanDirOption.AddAlias("-s");

var outputDirOption = new Option<string>(
    name: "--output",
    description: "Папка для збереження витягнутих файлів",
    getDefaultValue: () => Path.Combine(Directory.GetCurrentDirectory(), "Output"));
outputDirOption.AddAlias("-o");

var includeFormatsOption = new Option<string[]>(
    name: "--include",
    description: "Формати файлів для екстракції (наприклад: .locres .uasset). Якщо не вказано - витягуються всі файли")
{ AllowMultipleArgumentsPerToken = true };
includeFormatsOption.AddAlias("-i");

var excludeFormatsOption = new Option<string[]>(
    name: "--exclude-formats",
    description: "Формати файлів для ігнорування (наприклад: .uexp .ubulk)",
    getDefaultValue: () => Array.Empty<string>())
{ AllowMultipleArgumentsPerToken = true };
excludeFormatsOption.AddAlias("-ef");

var excludeFoldersOption = new Option<string[]>(
    name: "--exclude-folders",
    description: "Папки для ігнорування (наприклад: Engine/Content /Temp)",
    getDefaultValue: () => Array.Empty<string>())
{ AllowMultipleArgumentsPerToken = true };
excludeFoldersOption.AddAlias("-ex");

var rootCommand = new RootCommand("Утиліта для витягування файлів з Unreal Engine pak архівів");
rootCommand.AddOption(pakFileOption);
rootCommand.AddOption(scanDirOption);
rootCommand.AddOption(outputDirOption);
rootCommand.AddOption(includeFormatsOption);
rootCommand.AddOption(excludeFormatsOption);
rootCommand.AddOption(excludeFoldersOption);

rootCommand.SetHandler(async (pakFile, scanDir, outputDir, includeFormats, excludeFormats, excludeFolders) =>
{
    try
    {
        // Перевірка: мають бути вказані або --pak або --scan
        if (string.IsNullOrEmpty(pakFile) && string.IsNullOrEmpty(scanDir))
        {
            Console.WriteLine("Помилка: Вкажіть або --pak <файл> або --scan <папка>");
            return;
        }

        if (!string.IsNullOrEmpty(pakFile) && !string.IsNullOrEmpty(scanDir))
        {
            Console.WriteLine("Помилка: Вкажіть або --pak <файл> або --scan <папка>, але не обидва одночасно");
            return;
        }

        // Ініціалізація Zlib
        var zlibPath = Path.Combine(outputDir, ".data", ZlibHelper.DLL_NAME);
        if (!File.Exists(zlibPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(zlibPath)!);
            Console.WriteLine("Завантаження Zlib-ng...");
            if (!await ZlibHelper.DownloadDllAsync(zlibPath))
            {
                Console.WriteLine("Не вдалося завантажити Zlib-ng!");
                return;
            }
        }
        ZlibHelper.Initialize(zlibPath);
        Console.WriteLine("Zlib ініціалізовано\n");

        if (!string.IsNullOrEmpty(scanDir))
        {
            // Режим сканування
            await ScanAndExtractPaks(scanDir, outputDir, includeFormats, excludeFormats, excludeFolders);
        }
        else
        {
            // Режим екстракції одного файлу
            await ExtractSinglePak(pakFile!, outputDir, includeFormats, excludeFormats, excludeFolders);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nКритична помилка: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}, pakFileOption, scanDirOption, outputDirOption, includeFormatsOption, excludeFormatsOption, excludeFoldersOption);

return await rootCommand.InvokeAsync(args);

// Функція для екстракції одного pak файлу
async Task ExtractSinglePak(string pakFile, string outputDir, string[] includeFormats, string[] excludeFormats, string[] excludeFolders)
{
    if (!File.Exists(pakFile))
    {
        Console.WriteLine($"Помилка: Файл не знайдено: {pakFile}");
        return;
    }

    Console.WriteLine($"Відкриваю pak файл: {pakFile}");
    Console.WriteLine($"Папка виводу: {outputDir}");
    
    if (includeFormats.Length > 0)
    {
        Console.WriteLine($"Формати для витягування: {string.Join(", ", includeFormats)}");
    }
    else
    {
        Console.WriteLine("Формати для витягування: всі файли");
    }
    
    if (excludeFormats.Length > 0)
    {
        Console.WriteLine($"Ігноровані формати: {string.Join(", ", excludeFormats)}");
    }
    
    if (excludeFolders.Length > 0)
    {
        Console.WriteLine($"Ігноровані папки: {string.Join(", ", excludeFolders)}");
    }

    var versions = new VersionContainer(EGame.GAME_UE4_LATEST);
    var provider = new StreamedFileProvider("ExtractProvider", versions);
    provider.RegisterVfs(pakFile);
    provider.Mount();
    Console.WriteLine($"VFS змонтовано");
    Console.WriteLine($"Всього файлів в pak: {provider.Files.Count}\n");

    await ExtractFiles(provider, outputDir, includeFormats, excludeFormats, excludeFolders, pakFile);
}

// Функція для сканування і екстракції всіх pak файлів
async Task ScanAndExtractPaks(string scanDir, string outputDir, string[] includeFormats, string[] excludeFormats, string[] excludeFolders)
{
    if (!Directory.Exists(scanDir))
    {
        Console.WriteLine($"Помилка: Папка не знайдена: {scanDir}");
        return;
    }

    Console.WriteLine($"Сканую папку: {scanDir}");
    Console.WriteLine($"Папка виводу: {outputDir}");
    
    if (includeFormats.Length > 0)
    {
        Console.WriteLine($"Формати для витягування: {string.Join(", ", includeFormats)}");
    }
    else
    {
        Console.WriteLine("Формати для витягування: всі файли");
    }
    
    if (excludeFormats.Length > 0)
    {
        Console.WriteLine($"Ігноровані формати: {string.Join(", ", excludeFormats)}");
    }
    
    if (excludeFolders.Length > 0)
    {
        Console.WriteLine($"Ігноровані папки: {string.Join(", ", excludeFolders)}");
    }

    // Пошук всіх pak файлів
    var pakFiles = Directory.GetFiles(scanDir, "*.pak", SearchOption.AllDirectories);
    
    if (pakFiles.Length == 0)
    {
        Console.WriteLine($"Помилка: Не знайдено жодного pak файлу в {scanDir}");
        return;
    }

    Console.WriteLine($"\nЗнайдено {pakFiles.Length} pak файлів:\n");
    for (int i = 0; i < pakFiles.Length; i++)
    {
        Console.WriteLine($"  [{i + 1}] {Path.GetFileName(pakFiles[i])}");
    }
    Console.WriteLine();

    var versions = new VersionContainer(EGame.GAME_UE4_LATEST);
    int processedCount = 0;
    int totalSuccess = 0;
    int totalErrors = 0;

    foreach (var pakFile in pakFiles)
    {
        processedCount++;
        Console.WriteLine($"\n{'='} Обробляю [{processedCount}/{pakFiles.Length}] {Path.GetFileName(pakFile)}");
        
        try
        {
            var provider = new StreamedFileProvider($"ExtractProvider_{processedCount}", versions);
            provider.RegisterVfs(pakFile);
            provider.Mount();
            Console.WriteLine($"VFS змонтовано, файлів: {provider.Files.Count}");

            var (successCount, errorCount) = await ExtractFiles(provider, outputDir, includeFormats, excludeFormats, excludeFolders, pakFile);
            
            totalSuccess += successCount;
            totalErrors += errorCount;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка при обробці {pakFile}: {ex.Message}");
            totalErrors++;
        }
    }

    Console.WriteLine($"\n{'='} ПІДСУМОК СКАНУВАННЯ {'='}");
    Console.WriteLine($"Оброблено pak файлів: {processedCount}");
    Console.WriteLine($"Всього файлів екстраговано: {totalSuccess}");
    if (totalErrors > 0)
    {
        Console.WriteLine($"Помилок: {totalErrors}");
    }
    Console.WriteLine($"Файли збережено в: {outputDir}");
}

// Функція для екстракції файлів з провайдера
async Task<(int successCount, int errorCount)> ExtractFiles(
    StreamedFileProvider provider, 
    string outputDir, 
    string[] includeFormats, 
    string[] excludeFormats, 
    string[] excludeFolders,
    string pakFileName)
{
    // Нормалізація форматів
    var normalizedInclude = includeFormats
        .Select(f => f.StartsWith('.') ? f : '.' + f)
        .Select(f => f.ToLowerInvariant())
        .ToArray();
    
    var normalizedExcludeFormats = excludeFormats
        .Select(f => f.StartsWith('.') ? f : '.' + f)
        .Select(f => f.ToLowerInvariant())
        .ToArray();
    
    var normalizedExcludeFolders = excludeFolders
        .Select(f => f.Replace('\\', '/'))
        .ToArray();

    // Фільтрація файлів
    var filesToExtract = provider.Files.Keys.Where(filePath =>
    {
        var fileExt = Path.GetExtension(filePath).ToLowerInvariant();
        
        // Перевірка виключених форматів
        if (normalizedExcludeFormats.Length > 0 && normalizedExcludeFormats.Contains(fileExt))
        {
            return false;
        }
        
        // Перевірка виключених папок
        if (normalizedExcludeFolders.Length > 0)
        {
            var normalizedPath = filePath.Replace('\\', '/');
            if (normalizedExcludeFolders.Any(folder => 
                normalizedPath.Contains(folder, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }
        
        // Якщо вказані формати для включення - фільтруємо по них
        if (normalizedInclude.Length > 0)
        {
            return normalizedInclude.Contains(fileExt);
        }
        
        // Інакше включаємо всі файли
        return true;
    }).ToList();

    Console.WriteLine($"Знайдено {filesToExtract.Count} файлів для екстракції");

    if (!Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
    }

    int successCount = 0;
    int errorCount = 0;

    foreach (var file in filesToExtract)
    {
        try
        {
            var gameFile = provider.Files[file];
            using var archive = gameFile.CreateReader();
            var data = archive.ReadBytes((int)archive.Length);
            
            var relativePath = file.Replace('/', Path.DirectorySeparatorChar);
            var targetPath = Path.Combine(outputDir, relativePath);
            var targetDir = Path.GetDirectoryName(targetPath);
            
            if (!string.IsNullOrEmpty(targetDir) && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }
            
            File.WriteAllBytes(targetPath, data);
            successCount++;
            
            if (filesToExtract.Count <= 100 || successCount % 100 == 0)
            {
                Console.WriteLine($"[{successCount}/{filesToExtract.Count}] {file}");
            }
        }
        catch (Exception ex)
        {
            errorCount++;
            Console.WriteLine($"Помилка при обробці {file}: {ex.Message}");
        }
    }

    if (filesToExtract.Count > 0)
    {
        Console.WriteLine($"Успішно: {successCount}, Помилок: {errorCount}");
    }

    return await Task.FromResult((successCount, errorCount));
}