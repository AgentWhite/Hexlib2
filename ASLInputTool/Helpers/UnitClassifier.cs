using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Models.Components;

namespace ASLInputTool.Helpers
{
    /// <summary>
    /// Provides utility methods for classifying units based on their components.
    /// </summary>
    public static class UnitClassifier
    {
        /// <summary>
        /// Identifies the category of a unit based on its components.
        /// </summary>
        /// <param name="unit">The unit to classify.</param>
        /// <returns>A string representing the unit's category: "Hero", "Leader", "Infantry", "Equipment", or "Other".</returns>
        public static string GetCategory(Unit unit)
        {
            if (unit == null) return "Other";

            if (unit.HasComponent<HeroComponent>())
            {
                return "Hero";
            }

            if (unit.HasComponent<LeadershipComponent>())
            {
                return "Leader";
            }

            if (unit.HasComponent<InfantryComponent>())
            {
                return "Infantry";
            }

            if (unit.HasComponent<PortageComponent>())
            {
                return "Equipment";
            }

            return "Other";
        }
    }
}
