using ASL.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ASLInputTool.Infrastructure
{
    /// <summary>
    /// Draft interface for a repository that manages unit collections.
    /// Aims to decouple MainViewModel from direct persistence and collection management.
    /// </summary>
    public interface IUnitRepository
    {
        /// <summary>
        /// Gets all units managed by the repository.
        /// </summary>
        ObservableCollection<Unit> AllUnits { get; }

        /// <summary>
        /// Gets units filtered by their classification category.
        /// </summary>
        /// <param name="category">The category string (e.g., "Leader", "Hero").</param>
        IEnumerable<Unit> GetUnitsByCategory(string category);

        /// <summary>
        /// Initializes the repository with a collection of units.
        /// </summary>
        void Initialize(IEnumerable<Unit> units);

        /// <summary>
        /// Adds a new unit to the repository.
        /// </summary>
        void Add(Unit unit);

        /// <summary>
        /// Removes a unit from the repository.
        /// </summary>
        void Remove(Unit unit);

        /// <summary>
        /// Clears all units from the repository.
        /// </summary>
        void Clear();

        /// <summary>
        /// Performs any necessary cleanup or post-processing (e.g., fixing image paths).
        /// </summary>
        void ProcessData(string projectPath);
    }
}
