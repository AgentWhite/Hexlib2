using System;

namespace HexLib;

/// <summary>
/// Provides extension methods for the <see cref="PhysicalDirection"/> enum, particularly for rotation logic.
/// </summary>
public static class PhysicalDirectionExtensions
{
    private static readonly PhysicalDirection[] PointyToppedDirections = new[]
    {
        PhysicalDirection.NorthWest,
        PhysicalDirection.NorthEast,
        PhysicalDirection.East,
        PhysicalDirection.SouthEast,
        PhysicalDirection.SouthWest,
        PhysicalDirection.West
    };

    private static readonly PhysicalDirection[] FlatToppedDirections = new[]
    {
        PhysicalDirection.North,
        PhysicalDirection.NorthEast,
        PhysicalDirection.SouthEast,
        PhysicalDirection.South,
        PhysicalDirection.SouthWest,
        PhysicalDirection.NorthWest
    };

    /// <summary>
    /// Rotates the specified direction clockwise by a fixed number of 60-degree hex steps.
    /// </summary>
    /// <param name="direction">The starting direction.</param>
    /// <param name="steps">The number of steps to rotate clockwise (can be negative for counter-clockwise).</param>
    /// <param name="orientation">The visual orientation of the hex grid, determining which 6 directions are valid.</param>
    /// <returns>The newly facing direction.</returns>
    /// <exception cref="ArgumentException">Thrown if the provided direction is invalid for the specified orientation.</exception>
    public static PhysicalDirection RotateClockwise(this PhysicalDirection direction, int steps, HexTopOrientation orientation = HexTopOrientation.PointyTopped)
    {
        var array = orientation == HexTopOrientation.PointyTopped ? PointyToppedDirections : FlatToppedDirections;
        
        int currentIndex = Array.IndexOf(array, direction);
        if (currentIndex < 0)
        {
            throw new ArgumentException($"Direction {direction} is not valid for {orientation} hex grids.");
        }

        // Handle negative steps and wrap around using modulo math
        int newIndex = ((currentIndex + steps) % 6 + 6) % 6;
        
        return array[newIndex];
    }
}
