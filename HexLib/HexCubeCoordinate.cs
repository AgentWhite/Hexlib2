using System;

namespace HexLib;

/// <summary>
/// Represents a hexagonal grid coordinate using the cube coordinate system.
/// </summary>
/// <remarks>
/// In cube coordinates a hex position is represented by three axes (X, Y, Z)
/// constrained by the rule:
/// <code>
/// X + Y + Z = 0
/// </code>
/// This coordinate system simplifies many hex grid operations such as
/// distance calculations, neighbor lookup, and vector arithmetic.
/// </remarks>
public struct HexCubeCoordinate
{
    /// <summary>
    /// Gets the X component of the cube coordinate.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the Y component of the cube coordinate.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Gets the Z component of the cube coordinate.
    /// </summary>
    public int Z { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HexCubeCoordinate"/> struct.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="z">The Z coordinate.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the cube coordinate constraint (X + Y + Z = 0) is violated.
    /// </exception>
    public HexCubeCoordinate(int x, int y, int z)
    {
        if (x + y + z != 0)
            throw new ArgumentException("The sum of x, y and z must be 0.");

        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Adds two cube coordinates together.
    /// </summary>
    /// <param name="a">The first cube coordinate.</param>
    /// <param name="b">The second cube coordinate.</param>
    /// <returns>A new <see cref="HexCubeCoordinate"/> representing the sum of the coordinates.</returns>
    public static HexCubeCoordinate operator +(HexCubeCoordinate a, HexCubeCoordinate b)
    {
        return new HexCubeCoordinate(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    /// <summary>
    /// Subtracts one cube coordinate from another.
    /// </summary>
    /// <param name="a">The cube coordinate to subtract from.</param>
    /// <param name="b">The cube coordinate to subtract.</param>
    /// <returns>A new <see cref="HexCubeCoordinate"/> representing the difference between the coordinates.</returns>
    public static HexCubeCoordinate operator -(HexCubeCoordinate a, HexCubeCoordinate b)
    {
        return new HexCubeCoordinate(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
}