using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Models.Components;
using ASLInputTool.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace ASLInputTool.Infrastructure
{
    /// <summary>
    /// Concrete implementation of the unit repository.
    /// Manages the master collection of units and handles folder-based persistence.
    /// </summary>
    public class UnitRepository : IUnitRepository
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public UnitRepository()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

        /// <inheritdoc />
        public ObservableCollection<Unit> AllUnits { get; } = new();

        /// <inheritdoc />
        public IEnumerable<Unit> GetUnitsByCategory(string category)
        {
            return AllUnits.Where(u => UnitClassifier.GetCategory(u) == category);
        }

        /// <inheritdoc />
        public void Initialize(IEnumerable<Unit> units)
        {
            AllUnits.Clear();
            foreach (var unit in units)
            {
                AllUnits.Add(unit);
            }
        }

        /// <inheritdoc />
        public void Add(Unit unit)
        {
            if (unit != null && !AllUnits.Contains(unit))
            {
                AllUnits.Add(unit);
            }
        }

        /// <inheritdoc />
        public void Remove(Unit unit)
        {
            if (unit != null)
            {
                AllUnits.Remove(unit);
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            AllUnits.Clear();
        }

        /// <inheritdoc />
        public void ProcessData(string projectPath) { }

        /// <inheritdoc />
        public async Task SaveUnitsForModuleAsync(Module moduleType, string moduleFolderName)
        {
            string modulesFolder = SettingsManager.Instance.Settings.ModulesFolder;
            if (string.IsNullOrWhiteSpace(modulesFolder)) return;

            string modulePath = Path.Combine(modulesFolder, moduleFolderName);
            if (!Directory.Exists(modulePath)) Directory.CreateDirectory(modulePath);

            string imagesDir = Path.Combine(modulePath, "images");
            if (!Directory.Exists(imagesDir)) Directory.CreateDirectory(imagesDir);

            // Synchronize save operations to prevent IOException (file in use)
            await _saveSemaphore.WaitAsync();
            try
            {
                var moduleUnits = AllUnits.Where(u => u.Module == moduleType).ToList();
                HashSet<string> usedImages = new(StringComparer.OrdinalIgnoreCase);

                // Group units by their conceptual classification for separate file saving
                var leaders = moduleUnits.Where(u => u.IsLeader).ToList();
                var heroes = moduleUnits.Where(u => u.IsHero).ToList();
                var squads = moduleUnits.Where(u => u.IsSquad).ToList();
                var equipment = moduleUnits.Where(u => u.IsSupportWeapon).ToList();

                await SaveUnitCollectionAsync(leaders, Path.Combine(modulePath, "leaders.asl"), imagesDir, usedImages);
                await SaveUnitCollectionAsync(heroes, Path.Combine(modulePath, "heroes.asl"), imagesDir, usedImages);
                await SaveUnitCollectionAsync(squads, Path.Combine(modulePath, "squads.asl"), imagesDir, usedImages);
                await SaveUnitCollectionAsync(equipment, Path.Combine(modulePath, "equipment.asl"), imagesDir, usedImages);

                // Restore absolute paths in memory for all units in this module
                foreach (var unit in moduleUnits)
                {
                    FixUnitImagePaths(unit, modulePath);
                }
            }
            finally
            {
                _saveSemaphore.Release();
            }

            // Images cleanup is handled by module saving or could be triggered here if safe.
        }

        private async Task SaveUnitCollectionAsync(List<Unit> units, string filePath, string imagesDir, HashSet<string> usedImages)
        {
            if (!units.Any())
            {
                if (File.Exists(filePath)) File.Delete(filePath);
                return;
            }

            // Create a temporary list for serialization to avoid corrupting in-memory relative paths permanently
            // Actually, we modify them and then call FixUnitImagePaths in the parent caller.
            foreach (var unit in units)
            {
                unit.Visual.ImagePathFront = ProcessImage(unit.Visual.ImagePathFront, imagesDir, usedImages);
                unit.Visual.ImagePathBack = ProcessImage(unit.Visual.ImagePathBack, imagesDir, usedImages);

                var portage = unit.GetComponent<PortageComponent>();
                if (portage != null)
                {
                    portage.DismantledImage = ProcessImage(portage.DismantledImage, imagesDir, usedImages);
                }
            }

            string json = JsonSerializer.Serialize(units, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);
        }

        /// <inheritdoc />
        public async Task LoadUnitsForModuleAsync(string moduleFolderName)
        {
            string modulesFolder = SettingsManager.Instance.Settings.ModulesFolder;
            if (string.IsNullOrWhiteSpace(modulesFolder)) return;

            string modulePath = Path.Combine(modulesFolder, moduleFolderName);
            if (!Directory.Exists(modulePath)) return;

            await LoadUnitFileAsync(Path.Combine(modulePath, "leaders.asl"), modulePath);
            await LoadUnitFileAsync(Path.Combine(modulePath, "heroes.asl"), modulePath);
            await LoadUnitFileAsync(Path.Combine(modulePath, "squads.asl"), modulePath);
            await LoadUnitFileAsync(Path.Combine(modulePath, "equipment.asl"), modulePath);
        }

        private async Task LoadUnitFileAsync(string filePath, string modulePath)
        {
            if (!File.Exists(filePath)) return;

            try
            {
                string json = await File.ReadAllTextAsync(filePath);
                var units = JsonSerializer.Deserialize<List<Unit>>(json, _jsonOptions);
                if (units != null)
                {
                    foreach (var unit in units)
                    {
                        FixUnitImagePaths(unit, modulePath);
                        RestoreBackReferences(unit);
                        Add(unit);
                    }
                }
            }
            catch { /* Skip corrupted files */ }
        }

        /// <inheritdoc />
        public async Task DeleteUnitsForModuleAsync(string moduleFolderName)
        {
            string modulesFolder = SettingsManager.Instance.Settings.ModulesFolder;
            if (string.IsNullOrWhiteSpace(modulesFolder)) return;

            string modulePath = Path.Combine(modulesFolder, moduleFolderName);
            if (!Directory.Exists(modulePath)) return;

            string[] unitFiles = { "leaders.asl", "heroes.asl", "squads.asl", "equipment.asl" };
            foreach (var f in unitFiles)
            {
                string p = Path.Combine(modulePath, f);
                if (File.Exists(p)) File.Delete(p);
            }
        }

        private void RestoreBackReferences(Unit unit)
        {
            foreach (var comp in unit.Components)
            {
                comp.Initialize(unit);
            }
        }

        private void FixUnitImagePaths(Unit c, string moduleFolder)
        {
            if (c.Visual == null) c.Visual = new UnitVisual();

            if (!string.IsNullOrEmpty(c.Visual.ImagePathFront) && !Path.IsPathRooted(c.Visual.ImagePathFront))
                c.Visual.ImagePathFront = Path.GetFullPath(Path.Combine(moduleFolder, c.Visual.ImagePathFront!));

            if (!string.IsNullOrEmpty(c.Visual.ImagePathBack) && !Path.IsPathRooted(c.Visual.ImagePathBack))
                c.Visual.ImagePathBack = Path.GetFullPath(Path.Combine(moduleFolder, c.Visual.ImagePathBack!));

            var portage = c.GetComponent<PortageComponent>();
            if (portage != null && !string.IsNullOrEmpty(portage.DismantledImage) && !Path.IsPathRooted(portage.DismantledImage))
                portage.DismantledImage = Path.GetFullPath(Path.Combine(moduleFolder, portage.DismantledImage!));
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
    }
}
