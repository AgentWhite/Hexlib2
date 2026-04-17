namespace HexLib;

/// <summary>
/// A simple, framework-independent 2D point using doubles.
/// Used for geometric calculations before converting to UI-specific types.
/// </summary>
public struct Point2D
{
    public double X { get; }
    public double Y { get; }

    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
    }
}
