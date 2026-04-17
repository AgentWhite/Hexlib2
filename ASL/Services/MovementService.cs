using HexLib;
using ASL.Models.Board;

namespace ASL.Services;

/// <summary>
/// Provides utility methods for calculating movement costs in an ASL game.
/// </summary>
public static class MovementService
{
    /// <summary>
    /// Calculates the cost to move from hex 'from' to hex 'to'.
    /// Accounts for road bonuses if the connecting edge has a road.
    /// Accounts for wall/hedge penalties if the edge has them.
    /// </summary>
    public static double CalculateCost(
        Hex<ASLHexMetadata> from, 
        Hex<ASLHexMetadata> to, 
        ASLEdgeData? edge)
    {
        // 1. Initial cost based on target terrain
        double cost = GetTerrainBaseCost(to.Metadata?.Terrain ?? TerrainType.OpenGround);

        // Shellholes in Open Ground cost 2 MF (In ASL, they are effectively a different base cost for Open Ground)
        if (to.Metadata != null && to.Metadata.HasShellholes && to.Metadata.Terrain == TerrainType.OpenGround)
        {
            cost = 2.0;
        }

        // Rubble costs 3 MF
        if (to.Metadata != null && to.Metadata.Rubble != RubbleType.None)
        {
            cost = 3.0;
        }
        
        // 2. Elevation change penalty (Standard ASL: +2 MF per level up)
        if (to.Metadata != null && from.Metadata != null)
        {
            if (to.Metadata.Elevation > from.Metadata.Elevation)
            {
                cost += 2; 
            }
        }

        // 3. Road bonus
        // In ASL, if you move through a road hexside, you ignore the base terrain cost.
        if (edge != null)
        {
            if (edge.HasPavedRoad)
            {
                return 0.5; // Paved road movement cost
            }
            if (edge.HasDirtRoad)
            {
                return 1.0; // Dirt road movement cost
            }
        }

        // 4. Obstacle penalties (Walls/Hedges)
        if (edge != null)
        {
            if (edge.HasWall) cost += 1; // Wall penalty
            if (edge.HasHedge) cost += 1; // Hedge penalty
        }

        return cost;
    }

    private static double GetTerrainBaseCost(TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.Woods => 3,
            TerrainType.StoneBuilding => 3,
            TerrainType.WoodenBuilding => 2,
            TerrainType.Crag => 2,
            TerrainType.Marsh => 5,
            TerrainType.Water => 99,
            TerrainType.Pond => 99,
            TerrainType.Lumberyard => 2,
            _ => 1 // Open ground
        };
    }
}
