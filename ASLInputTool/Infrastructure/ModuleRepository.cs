using ASL.Models.Modules;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Concrete implementation of the module repository.
/// Manages the list of ASL modules and handles folder-based persistence.
/// </summary>
public class ModuleRepository : IModuleRepository
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ModuleRepository()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    /// <inheritdoc />
    public ObservableCollection<AslModule> AllModules { get; } = new();

    /// <inheritdoc />
    public void Initialize(IEnumerable<AslModule> modules)
    {
        AllModules.Clear();
        foreach (var module in modules)
        {
            AllModules.Add(module);
        }
    }

    /// <inheritdoc />
    public void Add(AslModule module)
    {
        if (!AllModules.Contains(module))
        {
            AllModules.Add(module);
        }
    }

    /// <inheritdoc />
    public void Remove(AslModule module)
    {
        AllModules.Remove(module);
    }

    /// <inheritdoc />
    public void Clear()
    {
        AllModules.Clear();
    }

    /// <inheritdoc />
    public void ProcessData(string projectPath) { }

    /// <inheritdoc />
    public async Task<IEnumerable<AslModule>> ScanAndLoadAsync()
    {
        var modules = new List<AslModule>();
        string baseFolder = SettingsManager.Instance.Settings.ModulesFolder;
        
        if (string.IsNullOrWhiteSpace(baseFolder) || !Directory.Exists(baseFolder))
            return modules;

        foreach (var dir in Directory.GetDirectories(baseFolder))
        {
            string modulePath = Path.Combine(dir, "module.asl");
            if (File.Exists(modulePath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(modulePath);
                    var module = JsonSerializer.Deserialize<AslModule>(json, _jsonOptions);
                    if (module != null)
                    {
                        FixModuleImagePaths(module, dir);
                        modules.Add(module);
                    }
                }
                catch { /* Skip corrupted files */ }
            }
        }
        return modules;
    }

    /// <inheritdoc />
    public async Task SaveToDiskAsync(AslModule module, string? originalName = null)
    {
        string baseFolder = SettingsManager.Instance.Settings.ModulesFolder;
        if (string.IsNullOrWhiteSpace(baseFolder)) return;

        // Handle Renaming
        if (!string.IsNullOrEmpty(originalName) && originalName != module.FullName)
        {
            string oldFolder = Path.Combine(baseFolder, originalName);
            string newFolder = Path.Combine(baseFolder, module.FullName);

            if (Directory.Exists(oldFolder) && !Directory.Exists(newFolder))
            {
                try { Directory.Move(oldFolder, newFolder); }
                catch (Exception ex) { throw new IOException($"Failed to rename module folder: {ex.Message}", ex); }
            }
        }

        string moduleFolder = Path.Combine(baseFolder, module.FullName);
        if (!Directory.Exists(moduleFolder)) Directory.CreateDirectory(moduleFolder);

        string imagesDir = Path.Combine(moduleFolder, "images");
        if (!Directory.Exists(imagesDir)) Directory.CreateDirectory(imagesDir);

        // Prepare a copy for saving with relative paths
        var saveCopy = new AslModule
        {
            FullName = module.FullName,
            Description = module.Description,
            Module = module.Module,
            IsFinished = module.IsFinished,
        };

        HashSet<string> usedImages = new(StringComparer.OrdinalIgnoreCase);
        saveCopy.FrontImage = ProcessImage(module.FrontImage, imagesDir, usedImages);
        saveCopy.BackImage = ProcessImage(module.BackImage, imagesDir, usedImages);

        // Cleanup unused images in the local images directory
        CleanupUnusedImages(imagesDir, usedImages);

        string json = JsonSerializer.Serialize(saveCopy, _jsonOptions);
        string moduleFilePath = Path.Combine(moduleFolder, "module.asl");
        await File.WriteAllTextAsync(moduleFilePath, json);

        // Update the original module's paths to point to the local copies
        module.FrontImage = saveCopy.FrontImage;
        module.BackImage = saveCopy.BackImage;
        FixModuleImagePaths(module, moduleFolder);
    }

    private string? ProcessImage(string? sourcePath, string targetDir, HashSet<string> usedImages)
    {
        if (string.IsNullOrEmpty(sourcePath)) return null;

        if (!Path.IsPathRooted(sourcePath) && sourcePath.StartsWith("images"))
        {
            usedImages.Add(Path.GetFileName(sourcePath));
            return sourcePath;
        }

        if (!File.Exists(sourcePath)) return sourcePath;

        string extension = Path.GetExtension(sourcePath);
        string fullSourcePath = Path.GetFullPath(sourcePath);
        string fullTargetDir = Path.GetFullPath(targetDir);

        if (fullSourcePath.StartsWith(fullTargetDir, StringComparison.OrdinalIgnoreCase) 
            && Guid.TryParse(Path.GetFileNameWithoutExtension(sourcePath), out _))
        {
            string fileName = Path.GetFileName(sourcePath);
            usedImages.Add(fileName);
            return Path.Combine("images", fileName);
        }

        string newFileName = $"{Guid.NewGuid()}{extension}";
        string destPath = Path.Combine(targetDir, newFileName);

        try
        {
            File.Copy(sourcePath, destPath, true);
            usedImages.Add(newFileName);
            return Path.Combine("images", newFileName);
        }
        catch { return sourcePath; }
    }

    private void CleanupUnusedImages(string imagesDir, HashSet<string> usedImages)
    {
        if (!Directory.Exists(imagesDir)) return;

        foreach (var file in Directory.GetFiles(imagesDir))
        {
            string fileName = Path.GetFileName(file);
            if (!usedImages.Contains(fileName))
            {
                try { File.Delete(file); }
                catch { /* Ignore cleanup errors */ }
            }
        }
    }

    private void FixModuleImagePaths(AslModule module, string moduleFolder)
    {
        if (!string.IsNullOrEmpty(module.FrontImage) && !Path.IsPathRooted(module.FrontImage))
            module.FrontImage = Path.GetFullPath(Path.Combine(moduleFolder, module.FrontImage));

        if (!string.IsNullOrEmpty(module.BackImage) && !Path.IsPathRooted(module.BackImage))
            module.BackImage = Path.GetFullPath(Path.Combine(moduleFolder, module.BackImage));
    }
}
