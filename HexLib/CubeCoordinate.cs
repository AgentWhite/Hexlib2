namespace HexLib;

/// <summary>
/// An immutable coordinate system for hexagonal grids using 3 axes (q, r, s) where q + r + s = 0.
/// Provides math operators and rotation translations.
/// </summary>
public readonly struct CubeCoordinate : IEquatable<CubeCoordinate>
{
    /// <summary>The Q (column-like) axis of the cube coordinate.</summary>
    public int Q { get; }
    
    /// <summary>The R (row-like) axis of the cube coordinate.</summary>
    public int R { get; }
    
    /// <summary>The S axis of the cube coordinate, balancing the sum to zero.</summary>
    public int S { get; }

    /// <summary>Gets a coordinate at (0,0,0).</summary>
    public static CubeCoordinate Zero => new CubeCoordinate(0, 0, 0);

    /// <summary>
    /// Initializes a new cube coordinate. Throws if the sum of axes is not 0.
    /// </summary>
    public CubeCoordinate(int q, int r, int s)
    {
        if (q + r + s != 0)
        {
            throw new ArgumentException($"Invalid cube coordinate: q + r + s must equal 0. Got {q} + {r} + {s} = {q + r + s}");
        }
        Q = q;
        R = r;
        S = s;
    }

    /// <summary>Adds two cube coordinates together.</summary>
    public static CubeCoordinate operator +(CubeCoordinate a, CubeCoordinate b) =>
        new CubeCoordinate(a.Q + b.Q, a.R + b.R, a.S + b.S);

    /// <summary>Subtracts one cube coordinate from another.</summary>
    public static CubeCoordinate operator -(CubeCoordinate a, CubeCoordinate b) =>
        new CubeCoordinate(a.Q - b.Q, a.R - b.R, a.S - b.S);

    /// <summary>Multiplies a cube coordinate by a scalar value.</summary>
    public static CubeCoordinate operator *(CubeCoordinate a, int multiplier) =>
        new CubeCoordinate(a.Q * multiplier, a.R * multiplier, a.S * multiplier);

    /// <summary>Calculates the Manhattan distance (in hexes) to another cube coordinate.</summary>
    /// <param name="other">The target coordinate.</param>
    /// <returns>The distance in hexes.</returns>
    public int DistanceTo(CubeCoordinate other) =>
        (Math.Abs(Q - other.Q) + Math.Abs(R - other.R) + Math.Abs(S - other.S)) / 2;

    /// <summary>Rotates the coordinate 60 degrees clockwise around the origin.</summary>
    /// <remarks>
    /// Swaps and negates axes: (q, r, s) becomes (-r, -s, -q).
    /// This maintains the invariant q + r + s = 0.
    /// </remarks>
    public CubeCoordinate Rotate60() => new CubeCoordinate(-R, -S, -Q);
    
    /// <summary>Rotates the coordinate 120 degrees clockwise around the origin.</summary>
    public CubeCoordinate Rotate120() => new CubeCoordinate(S, Q, R);
    
    /// <summary>Rotates the coordinate 180 degrees around the origin.</summary>
    public CubeCoordinate Rotate180() => new CubeCoordinate(-Q, -R, -S);
    
    /// <summary>Rotates the coordinate 240 degrees (120 deg counter-clockwise) around the origin.</summary>
    public CubeCoordinate Rotate240() => new CubeCoordinate(R, S, Q);
    
    /// <summary>Rotates the coordinate 300 degrees (60 deg counter-clockwise) around the origin.</summary>
    public CubeCoordinate Rotate300() => new CubeCoordinate(-S, -Q, -R);

    /// <inheritdoc />
    public bool Equals(CubeCoordinate other) => Q == other.Q && R == other.R && S == other.S;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CubeCoordinate c && Equals(c);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Q, R, S);

    /// <summary>Determines if two cube coordinates are equal.</summary>
    public static bool operator ==(CubeCoordinate left, CubeCoordinate right) => left.Equals(right);

    /// <summary>Determines if two cube coordinates are not equal.</summary>
    public static bool operator !=(CubeCoordinate left, CubeCoordinate right) => !left.Equals(right);

    /// <summary>
    /// Static collection of unit vectors for the 6 neighbor directions.
    /// Order is East, NorthEast, NorthWest, West, SouthWest, SouthEast (standard Red Blob mapping).
    /// </summary>
    public static readonly CubeCoordinate[] Directions = new[]
    {
        new CubeCoordinate(1, 0, -1), 
        new CubeCoordinate(1, -1, 0), 
        new CubeCoordinate(0, -1, 1),
        new CubeCoordinate(-1, 0, 1), 
        new CubeCoordinate(-1, 1, 0), 
        new CubeCoordinate(0, 1, -1)
    };

    /// <summary>
    /// Gets the adjacent neighbor coordinate in the specified direction index (0-5).
    /// </summary>
    /// <param name="direction">The direction index to get the neighbor for.</param>
    /// <returns>The neighboring cube coordinate.</returns>
    public CubeCoordinate GetNeighbor(int direction)
    {
        var dir = (direction % 6 + 6) % 6;
        return this + Directions[dir];
    }

    /// <summary>
    /// Gets the adjacent neighbor coordinate in the specified physical direction.
    /// </summary>
    public CubeCoordinate GetNeighbor(PhysicalDirection physicalDir, HexTopOrientation orientation = HexTopOrientation.PointyTopped)
    {
        if (orientation == HexTopOrientation.PointyTopped)
        {
            return physicalDir switch
            {
                PhysicalDirection.East => GetNeighbor(0),
                PhysicalDirection.NorthEast => GetNeighbor(1),
                PhysicalDirection.NorthWest => GetNeighbor(2),
                PhysicalDirection.West => GetNeighbor(3),
                PhysicalDirection.SouthWest => GetNeighbor(4),
                PhysicalDirection.SouthEast => GetNeighbor(5),
                _ => throw new ArgumentException($"Direction {physicalDir} is invalid for PointyTopped grid.")
            };
        }
        else
        {
            return physicalDir switch
            {
                PhysicalDirection.NorthEast => GetNeighbor(0),
                PhysicalDirection.North => GetNeighbor(1),
                PhysicalDirection.NorthWest => GetNeighbor(2),
                PhysicalDirection.SouthWest => GetNeighbor(3),
                PhysicalDirection.South => GetNeighbor(4),
                PhysicalDirection.SouthEast => GetNeighbor(5),
                _ => throw new ArgumentException($"Direction {physicalDir} is invalid for FlatTopped grid.")
            };
        }
    }

    /// <summary>
    /// Linearly interpolates between two coordinates. 
    /// Used for finding points along a line between two hexes.
    /// </summary>
    public FractionalHex Lerp(CubeCoordinate target, double t)
    {
        return new FractionalHex(
            Q + (target.Q - Q) * t,
            R + (target.R - R) * t,
            S + (target.S - S) * t
        );
    }
}
