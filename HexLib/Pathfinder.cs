using System;
using System.Collections.Generic;
using System.Linq;

namespace HexLib;

/// <summary>
/// Provides a generic implementation of the A* pathfinding algorithm for hexagonal grids.
/// </summary>
public static class Pathfinder
{
    /// <summary>
    /// Searches for the shortest path between two hexes using a custom cost logic.
    /// </summary>
    /// <typeparam name="TH">The hex metadata type.</typeparam>
    /// <typeparam name="TE">The edge data type.</typeparam>
    /// <param name="board">The board to search on.</param>
    /// <param name="start">The starting coordinate.</param>
    /// <param name="goal">The target coordinate.</param>
    /// <param name="costFunc">A function that returns the cost of moving from one hex to an adjacent neighbor. Return <c>float.PositiveInfinity</c> for impassable boundaries.</param>
    /// <returns>A list of coordinates representing the path from start to goal (inclusive), or null if no path exists.</returns>
    public static List<CubeCoordinate>? FindPath<TH, TE>(
        Board<TH, TE> board,
        CubeCoordinate start,
        CubeCoordinate goal,
        Func<CubeCoordinate, CubeCoordinate, float> costFunc)
    {
        var frontier = new PriorityQueue<CubeCoordinate, float>();
        frontier.Enqueue(start, 0);

        var cameFrom = new Dictionary<CubeCoordinate, CubeCoordinate?>();
        var costSoFar = new Dictionary<CubeCoordinate, float>();

        cameFrom[start] = null;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current == goal)
            {
                return ReconstructPath(cameFrom, goal);
            }

            foreach (var neighbor in GetValidNeighbors(board, current))
            {
                float stepCost = costFunc(current, neighbor);
                if (float.IsInfinity(stepCost)) continue;

                float newCost = costSoFar[current] + stepCost;
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    float priority = newCost + current.DistanceTo(goal); // Simple A* heuristic
                    frontier.Enqueue(neighbor, priority);
                    cameFrom[neighbor] = current;
                }
            }
        }

        return null;
    }

    private static IEnumerable<CubeCoordinate> GetValidNeighbors<TH, TE>(Board<TH, TE> board, CubeCoordinate current)
    {
        for (int i = 0; i < 6; i++)
        {
            var neighbor = current.GetNeighbor(i);
            if (board.Hexes.ContainsKey(neighbor))
            {
                yield return neighbor;
            }
        }
    }

    private static List<CubeCoordinate> ReconstructPath(Dictionary<CubeCoordinate, CubeCoordinate?> cameFrom, CubeCoordinate goal)
    {
        var path = new List<CubeCoordinate>();
        var current = (CubeCoordinate?)goal;
        while (current != null)
        {
            path.Add(current.Value);
            current = cameFrom[current.Value];
        }
        path.Reverse();
        return path;
    }
}

// Simple PriorityQueue implementation as it's not native in all .NET versions
internal class PriorityQueue<TItem, TPriority> where TPriority : IComparable<TPriority>
{
    private readonly List<(TItem item, TPriority priority)> _elements = new();

    public int Count => _elements.Count;

    public void Enqueue(TItem item, TPriority priority)
    {
        _elements.Add((item, priority));
    }

    public TItem Dequeue()
    {
        int bestIndex = 0;
        for (int i = 1; i < _elements.Count; i++)
        {
            if (_elements[i].priority.CompareTo(_elements[bestIndex].priority) < 0)
            {
                bestIndex = i;
            }
        }
        TItem bestItem = _elements[bestIndex].item;
        _elements.RemoveAt(bestIndex);
        return bestItem;
    }
}
