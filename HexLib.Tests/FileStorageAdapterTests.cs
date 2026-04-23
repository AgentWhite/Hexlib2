using HexLib.Persistence;
using System.IO;

namespace HexLib.Tests;

public class FileStorageAdapterTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileStorageAdapter _adapter;

    public FileStorageAdapterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _adapter = new FileStorageAdapter(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    // ── SaveAsync / LoadAsync round-trip ──────────────────────────────────────

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_ReturnsOriginalData()
    {
        await _adapter.SaveAsync("units/hero", "{\"name\":\"Sgt Rock\"}");

        var loaded = await _adapter.LoadAsync("units/hero");

        Assert.Equal("{\"name\":\"Sgt Rock\"}", loaded);
    }

    [Fact]
    public async Task LoadAsync_NonExistentKey_ReturnsNull()
    {
        var result = await _adapter.LoadAsync("units/ghost");

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAsync_CreatesJsonFileOnDisk()
    {
        await _adapter.SaveAsync("boards/test", "data");

        string expectedPath = Path.Combine(_tempDir, "boards", "test.json");
        Assert.True(File.Exists(expectedPath));
    }

    [Fact]
    public async Task SaveAsync_KeyAlreadyHasJsonExtension_DoesNotDoubleAppend()
    {
        await _adapter.SaveAsync("boards/test.json", "data");

        string expectedPath = Path.Combine(_tempDir, "boards", "test.json");
        Assert.True(File.Exists(expectedPath));

        // The file must not exist with a doubled extension
        string doubledPath = Path.Combine(_tempDir, "boards", "test.json.json");
        Assert.False(File.Exists(doubledPath));
    }

    [Fact]
    public async Task SaveAsync_Overwrites_ExistingFile()
    {
        await _adapter.SaveAsync("units/squad", "original");
        await _adapter.SaveAsync("units/squad", "updated");

        var result = await _adapter.LoadAsync("units/squad");
        Assert.Equal("updated", result);
    }

    // ── ListKeysAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ListKeysAsync_ReturnsKeysForCategory()
    {
        await _adapter.SaveAsync("units/alpha", "a");
        await _adapter.SaveAsync("units/bravo", "b");

        var keys = (await _adapter.ListKeysAsync("units")).ToList();

        Assert.Contains("alpha", keys);
        Assert.Contains("bravo", keys);
    }

    [Fact]
    public async Task ListKeysAsync_NonExistentCategory_ReturnsEmpty()
    {
        var keys = await _adapter.ListKeysAsync("nonexistent");

        Assert.Empty(keys);
    }

    [Fact]
    public async Task ListKeysAsync_OnlyReturnsJsonFiles()
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, "units"));
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "units", "alpha.json"), "{}");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "units", "readme.txt"), "txt");

        var keys = (await _adapter.ListKeysAsync("units")).ToList();

        Assert.Contains("alpha", keys);
        Assert.DoesNotContain("readme", keys);
        Assert.DoesNotContain("readme.txt", keys);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingKey_RemovesFile()
    {
        await _adapter.SaveAsync("units/delete-me", "bye");

        await _adapter.DeleteAsync("units/delete-me");

        var result = await _adapter.LoadAsync("units/delete-me");
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentKey_DoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(() => _adapter.DeleteAsync("units/ghost"));
        Assert.Null(ex);
    }

    // ── Directory creation ────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_CreatesNestedCategoryDirectory()
    {
        string categoryPath = Path.Combine(_tempDir, "deep", "nested");
        Assert.False(Directory.Exists(categoryPath));

        await _adapter.SaveAsync("deep/nested/item", "data");

        Assert.True(File.Exists(Path.Combine(_tempDir, "deep", "nested", "item.json")));
    }

    [Fact]
    public void Constructor_NonExistentDirectory_CreatesIt()
    {
        string newDir = Path.Combine(_tempDir, "newroot");
        Assert.False(Directory.Exists(newDir));

        _ = new FileStorageAdapter(newDir);

        Assert.True(Directory.Exists(newDir));
    }
}
