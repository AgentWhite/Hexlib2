using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HexLib.Persistence;

/// <summary>
/// A file-system based implementation of <see cref="IStorageAdapter"/>.
/// </summary>
public class FileStorageAdapter : IStorageAdapter
{
    private readonly string _rootDirectory;

    public FileStorageAdapter(string rootDirectory)
    {
        _rootDirectory = rootDirectory;
        if (!Directory.Exists(_rootDirectory))
        {
            Directory.CreateDirectory(_rootDirectory);
        }
    }

    public async Task SaveAsync(string key, string data)
    {
        string filePath = GetFilePath(key);
        string? directory = Path.GetDirectoryName(filePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        await File.WriteAllTextAsync(filePath, data);
    }

    public async Task<string?> LoadAsync(string key)
    {
        string filePath = GetFilePath(key);
        if (!File.Exists(filePath)) return null;
        return await File.ReadAllTextAsync(filePath);
    }

    public Task<IEnumerable<string>> ListKeysAsync(string category)
    {
        string categoryPath = Path.Combine(_rootDirectory, category);
        if (!Directory.Exists(categoryPath)) return Task.FromResult(Enumerable.Empty<string>());

        var files = Directory.GetFiles(categoryPath, "*.json")
                             .Select(f => Path.GetFileNameWithoutExtension(f));
        return Task.FromResult(files);
    }

    public Task DeleteAsync(string key)
    {
        string filePath = GetFilePath(key);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }

    private string GetFilePath(string key)
    {
        // Simple mapping: category/filename or just filename
        // Ensure we append .json extension for standard file handling
        if (!key.EndsWith(".json")) key += ".json";
        return Path.Combine(_rootDirectory, key);
    }
}
