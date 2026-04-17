namespace HexLib;

/// <summary>
/// A hex coordinate with fractional components, used during floating-point geometric translations.
/// Must be rounded to a <see cref="CubeCoordinate"/> for discrete grid operations.
/// </summary>
public struct FractionalHex
{
    public double Q { get; }
    public double R { get; }
    public double S { get; }

    public FractionalHex(double q, double r, double s)
    {
        Q = q;
        R = r;
        S = s;
    }

    /// <summary>
    /// Rounds the fractional coordinate to the nearest discrete cube coordinate.
    /// This is the standard "Hex Rounding" algorithm essential for hit testing.
    /// </summary>
    public CubeCoordinate Round()
    {
        int q = (int)Math.Round(Q);
        int r = (int)Math.Round(R);
        int s = (int)Math.Round(S);

        double qDiff = Math.Abs(q - Q);
        double rDiff = Math.Abs(r - R);
        double sDiff = Math.Abs(s - S);

        if (qDiff > rDiff && qDiff > sDiff)
        {
            q = -r - s;
        }
        else if (rDiff > sDiff)
        {
            r = -q - s;
        }
        else
        {
            s = -q - r;
        }

        return new CubeCoordinate(q, r, s);
    }
}
