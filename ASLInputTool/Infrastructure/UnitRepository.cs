using ASL.Models;
using ASL.Models.Components;
using ASLInputTool.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ASLInputTool.Infrastructure
{
    /// <summary>
    /// Concrete implementation of the unit repository.
    /// Manages the master collection of units and handles data processing like image path normalization.
    /// </summary>
    public class UnitRepository : IUnitRepository
    {
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
        public void ProcessData(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath)) return;

            string projectDir = Path.GetDirectoryName(projectPath) ?? string.Empty;
            foreach (var c in AllUnits)
            {
                FixUnitImagePaths(c, projectDir);
            }
        }

        /// <summary>
        /// Normalizes image paths for a unit relative to the project directory.
        /// </summary>
        private void FixUnitImagePaths(Unit c, string projectDir)
        {
            if (!string.IsNullOrEmpty(c.ImagePathFront) && !Path.IsPathRooted(c.ImagePathFront))
                c.ImagePathFront = Path.GetFullPath(Path.Combine(projectDir, c.ImagePathFront!));

            if (!string.IsNullOrEmpty(c.ImagePathBack) && !Path.IsPathRooted(c.ImagePathBack))
                c.ImagePathBack = Path.GetFullPath(Path.Combine(projectDir, c.ImagePathBack!));

            var portage = c.GetComponent<PortageComponent>();
            if (portage != null && !string.IsNullOrEmpty(portage.DismantledImage) && !Path.IsPathRooted(portage.DismantledImage))
                portage.DismantledImage = Path.GetFullPath(Path.Combine(projectDir, portage.DismantledImage!));
        }
    }
}
