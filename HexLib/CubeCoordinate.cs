namespace HexLib;

/// <summary>
/// An immutable coordinate system for hexagonal grids using 3 axes (q, r, s) where q + r + s = 0.
/// Provides math operators and rotation translations.
/// </summary>
public readonly struct CubeCoordinate : IEquatable<CubeCoordinate>
{
    public int Q { get; }
    public int R { get; }
    public int S { get; }

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

    public static CubeCoordinate operator +(CubeCoordinate a, CubeCoordinate b) =>
        new CubeCoordinate(a.Q + b.Q, a.R + b.R, a.S + b.S);

    public static CubeCoordinate operator -(CubeCoordinate a, CubeCoordinate b) =>
        new CubeCoordinate(a.Q - b.Q, a.R - b.R, a.S - b.S);

    public static CubeCoordinate operator *(CubeCoordinate a, int multiplier) =>
        new CubeCoordinate(a.Q * multiplier, a.R * multiplier, a.S * multiplier);

    public int DistanceTo(CubeCoordinate other) =>
        (Math.Abs(Q - other.Q) + Math.Abs(R - other.R) + Math.Abs(S - other.S)) / 2;

    public CubeCoordinate Rotate60() => new CubeCoordinate(-R, -S, -Q);
    public CubeCoordinate Rotate120() => new CubeCoordinate(S, Q, R);
    public CubeCoordinate Rotate180() => new CubeCoordinate(-Q, -R, -S);
    public CubeCoordinate Rotate240() => new CubeCoordinate(R, S, Q);
    public CubeCoordinate Rotate300() => new CubeCoordinate(-S, -Q, -R);

    public bool Equals(CubeCoordinate other) => Q == other.Q && R == other.R && S == other.S;

    public override bool Equals(object? obj) => obj is CubeCoordinate c && Equals(c);

    public override int GetHashCode() => HashCode.Combine(Q, R, S);

    public static bool operator ==(CubeCoordinate left, CubeCoordinate right) => left.Equals(right);

    public static bool operator !=(CubeCoordinate left, CubeCoordinate right) => !left.Equals(right);

    private static readonly CubeCoordinate[] Directions = new[]
    {
        new CubeCoordinate(1, 0, -1), 
        new CubeCoordinate(1, -1, 0), 
        new CubeCoordinate(0, -1, 1),
        new CubeCoordinate(-1, 0, 1), 
        new CubeCoordinate(-1, 1, 0), 
        new CubeCoordinate(0, 1, -1)
    };

    public CubeCoordinate GetNeighbor(int direction)
    {
        // Handle negative directions using modulo arithmetic
        var dir = (direction % 6 + 6) % 6;
        return this + Directions[dir];
    }
}
