using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Components;
using ASL.Persistence;
using HexLib.Persistence;
using System.IO;

namespace ASL.Tests;

public class SaveManagerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _projectFile;
    private readonly ASLSaveManager _manager;

    public SaveManagerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _projectFile = Path.Combine(_tempDir, "project.asl");
        _manager = new ASLSaveManager(new FileStorageAdapter(_tempDir));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreateTempImage(string extension = ".png")
    {
        string path = Path.Combine(_tempDir, $"src_{Guid.NewGuid()}{extension}");
        File.WriteAllBytes(path, new byte[] { 0x89, 0x50, 0x4E, 0x47 }); // PNG header stub
        return path;
    }

    // ── External absolute path is copied and relativized ─────────────────────

    [Fact]
    public void PrepareForSaving_ExternalImage_CopiedToImagesDir()
    {
        string srcImage = CreateTempImage();
        var project = new ASLProject
        {
            Counters = { new Unit { Name = "A", Visual = new UnitVisual { ImagePathFront = srcImage } } }
        };

        var result = _manager.PrepareProjectForSaving(project, _projectFile);

        string? front = result.Counters[0].Visual.ImagePathFront;
        Assert.NotNull(front);
        Assert.StartsWith("images", front);
        string copiedFile = Path.Combine(_tempDir, front);
        Assert.True(File.Exists(copiedFile));
    }

    [Fact]
    public void PrepareForSaving_ExternalImage_HasGuidFilename()
    {
        string srcImage = CreateTempImage(".png");
        var project = new ASLProject
        {
            Counters = { new Unit { Name = "A", Visual = new UnitVisual { ImagePathFront = srcImage } } }
        };

        var result = _manager.PrepareProjectForSaving(project, _projectFile);

        string fileName = Path.GetFileNameWithoutExtension(result.Counters[0].Visual.ImagePathFront!
            .Replace("images" + Path.DirectorySeparatorChar, "")
            .Replace("images/", ""));
        Assert.True(Guid.TryParse(fileName, out _), $"Filename '{fileName}' is not a GUID.");
    }

    // ── Already-relative images/ path is left alone ───────────────────────────

    [Fact]
    public void PrepareForSaving_AlreadyRelativePath_NotChanged()
    {
        var project = new ASLProject
        {
            Counters = { new Unit { Name = "A", Visual = new UnitVisual { ImagePathFront = "images/already.png" } } }
        };

        var result = _manager.PrepareProjectForSaving(project, _projectFile);

        Assert.Equal("images/already.png", result.Counters[0].Visual.ImagePathFront);
    }

    // ── Image already in target dir with GUID name ────────────────────────────

    [Fact]
    public void PrepareForSaving_ImageAlreadyInTargetDirWithGuid_Relativized()
    {
        string imagesDir = Path.Combine(_tempDir, "images");
        Directory.CreateDirectory(imagesDir);
        string guidName = $"{Guid.NewGuid()}.png";
        string existingPath = Path.Combine(imagesDir, guidName);
        File.WriteAllBytes(existingPath, new byte[] { 1 });

        var project = new ASLProject
        {
            Counters = { new Unit { Name = "A", Visual = new UnitVisual { ImagePathFront = existingPath } } }
        };

        var result = _manager.PrepareProjectForSaving(project, _projectFile);

        Assert.Equal(Path.Combine("images", guidName), result.Counters[0].Visual.ImagePathFront);
    }

    // ── Non-existent file path is returned unchanged ─────────────────────────

    [Fact]
    public void PrepareForSaving_NonexistentFile_PathReturnedUnchanged()
    {
        string ghost = Path.Combine(_tempDir, "ghost.png");
        var project = new ASLProject
        {
            Counters = { new Unit { Name = "A", Visual = new UnitVisual { ImagePathFront = ghost } } }
        };

        var result = _manager.PrepareProjectForSaving(project, _projectFile);

        Assert.Equal(ghost, result.Counters[0].Visual.ImagePathFront);
    }

    // ── Unused images in images/ dir are cleaned up ───────────────────────────

    [Fact]
    public void PrepareForSaving_UnusedExistingImages_Deleted()
    {
        string imagesDir = Path.Combine(_tempDir, "images");
        Directory.CreateDirectory(imagesDir);
        string orphan = Path.Combine(imagesDir, $"{Guid.NewGuid()}.png");
        File.WriteAllBytes(orphan, new byte[] { 1 });

        // Project has no images — orphan should be deleted
        var project = new ASLProject();

        _manager.PrepareProjectForSaving(project, _projectFile);

        Assert.False(File.Exists(orphan));
    }

    // ── Counter back image and portage image ─────────────────────────────────

    [Fact]
    public void PrepareForSaving_BackImage_AlsoCopied()
    {
        string src = CreateTempImage();
        var unit = new Unit { Name = "A", Visual = new UnitVisual { ImagePathBack = src } };
        var project = new ASLProject { Counters = { unit } };

        var result = _manager.PrepareProjectForSaving(project, _projectFile);

        Assert.StartsWith("images", result.Counters[0].Visual.ImagePathBack);
    }

    [Fact]
    public void PrepareForSaving_PortageImage_Processed()
    {
        string src = CreateTempImage();
        var unit = new Unit { Name = "A" };
        unit.AddComponent(new PortageComponent { DismantledImage = src });
        var project = new ASLProject { Counters = { unit } };

        var result = _manager.PrepareProjectForSaving(project, _projectFile);

        var portage = result.Counters[0].GetComponent<PortageComponent>();
        Assert.NotNull(portage);
        Assert.StartsWith("images", portage!.DismantledImage);
    }

    // ── Scenario images ───────────────────────────────────────────────────────

    [Fact]
    public void PrepareForSaving_ScenarioImage_Processed()
    {
        string src = CreateTempImage();
        var project = new ASLProject
        {
            Scenarios = { new Scenario { Name = "S1", ImagePath = src } }
        };

        var result = _manager.PrepareProjectForSaving(project, _projectFile);

        Assert.StartsWith("images", result.Scenarios[0].ImagePath);
    }

    // ── Module images ─────────────────────────────────────────────────────────

    [Fact]
    public void PrepareForSaving_ModuleFrontAndBackImages_Processed()
    {
        string front = CreateTempImage();
        string back = CreateTempImage();
        var project = new ASLProject
        {
            Modules = { new AslModule { FullName = "M1", FrontImage = front, BackImage = back } }
        };

        var result = _manager.PrepareProjectForSaving(project, _projectFile);

        Assert.StartsWith("images", result.Modules[0].FrontImage);
        Assert.StartsWith("images", result.Modules[0].BackImage);
    }

    // ── Source project is not mutated ─────────────────────────────────────────

    [Fact]
    public void PrepareForSaving_DoesNotMutateSourceProject()
    {
        string src = CreateTempImage();
        var unit = new Unit { Name = "A", Visual = new UnitVisual { ImagePathFront = src } };
        var project = new ASLProject { Counters = { unit } };
        string originalPath = unit.Visual.ImagePathFront;

        _manager.PrepareProjectForSaving(project, _projectFile);

        Assert.Equal(originalPath, unit.Visual.ImagePathFront);
    }

    // ── Serialize / Deserialize round-trip ────────────────────────────────────

    [Fact]
    public void SerializeDeserialize_PreservesUnitComponents()
    {
        var unit = new Unit { Name = "TestLeader", UnitType = ASL.Models.Units.UnitType.SMC };
        unit.AddComponent(new LeadershipComponent { Leadership = -1 });
        unit.AddComponent(new InfantryComponent { Morale = 9, Scale = InfantryScale.SMC });
        var project = new ASLProject { Counters = { unit } };

        string json = _manager.SerializeProject(project);
        var loaded = _manager.DeserializeProject(json);

        Assert.NotNull(loaded);
        Assert.Single(loaded!.Counters);
        var loadedUnit = loaded.Counters[0];
        Assert.Equal("TestLeader", loadedUnit.Name);
        Assert.True(loadedUnit.IsLeader);
        Assert.Equal(-1, loadedUnit.Leadership?.Leadership);
    }

    [Fact]
    public void DeserializeProject_EmptyJson_ReturnsNull()
    {
        var result = _manager.DeserializeProject(string.Empty);
        Assert.Null(result);
    }
}
