using HexLib;

namespace ASL;

public static class MovementCalculator
{
    /// <summary>
    /// Calculates the cost to move from hex 'from' to hex 'to'.
    /// Accounts for road bonuses if both hexes and the connecting edge have roads.
    /// Accounts for wall/hedge penalties if the edge has them.
    /// </summary>
    public static int CalculateCost(
        Hex<ASLHexMetadata> from, 
        Hex<ASLHexMetadata> to, 
        ASLEdgeData? edge)
    {
        // 1. Initial cost based on target terrain
        int cost = GetTerrainBaseCost(to.Metadata?.Terrain ?? TerrainType.OpenGround);
        
        // 2. Elevation change penalty
        if (to.Metadata != null && from.Metadata != null)
        {
            if (to.Metadata.Elevation > from.Metadata.Elevation)
            {
                cost += 2; // Extra cost to go uphill
            }
        }

        // 3. Road bonus
        // In ASL, if you move from road hex to road hex through a road hexside, cost is usually 1/2 or 1.
        bool isRoadMovement = from.Metadata?.Terrain == TerrainType.Road && 
                              to.Metadata?.Terrain == TerrainType.Road && 
                              (edge?.HasRoad ?? false);
        
        if (isRoadMovement)
        {
            return 1; // Simplified road movement cost
        }

        // 4. Obstacle penalties (Walls/Hedges)
        if (edge != null)
        {
            if (edge.HasWall) cost += 1; // Wall penalty
            if (edge.HasHedge) cost += 1; // Hedge penalty
        }

        return cost;
    }

    private static int GetTerrainBaseCost(TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Woods => 3,
            TerrainType.Building => 2,
            TerrainType.StoneBuilding => 3,
            TerrainType.Marsh => 5,
            TerrainType.Water => 99, // Impassable for infantry
            _ => 1 // Open ground / Road
        };
    }
}
