namespace HexLib;

/// <summary>
/// A hex coordinate with fractional components, used during floating-point geometric translations.
/// Must be rounded to a <see cref="CubeCoordinate"/> for discrete grid operations.
/// </summary>
public struct FractionalHex
{
    /// <summary>Gets the Q (axial column) component.</summary>
    public double Q { get; }
    /// <summary>Gets the R (axial row) component.</summary>
    public double R { get; }
    /// <summary>Gets the S (derived third axial) component.</summary>
    public double S { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FractionalHex"/> struct.
    /// </summary>
    /// <param name="q">The Q coordinate.</param>
    /// <param name="r">The R coordinate.</param>
    /// <param name="s">The S coordinate.</param>
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
