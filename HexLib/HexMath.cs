namespace HexLib;

/// <summary>
/// Helper to handle coordinate conversions for hexes, supporting both pointy-topped (odd-R) and flat-topped (odd-Q) layouts.
/// </summary>
public static class HexMath
{
    /// <summary>
    /// Converts cube coordinates to offset coordinates (column, row).
    /// </summary>
    /// <param name="cube">The cube coordinate to convert.</param>
    /// <param name="orientation">The top orientation of the hex grid.</param>
    /// <param name="useEvenStagger">True if even columns/rows are shifted down (Flat/Pointy).</param>
    /// <returns>A tuple containing the mapped column and row.</returns>
    public static (int col, int row) ToOffset(this CubeCoordinate cube, HexTopOrientation orientation = HexTopOrientation.PointyTopped, bool useEvenStagger = false)
    {
        if (orientation == HexTopOrientation.PointyTopped)
        {
            // PointyTopped (Odd-R or Even-R):
            // The row (R) is mapped directly. 
            // The column is adjusted based on the row parity to account for the zig-zag offset.
            // (cube.R & 1) extracts the parity (0 for even rows, 1 for odd).
            int col = cube.Q + (cube.R + (useEvenStagger ? (cube.R & 1) : -(cube.R & 1))) / 2;
            int row = cube.R;
            return (col, row);
        }
        else
        {
            // FlatTopped (Odd-Q or Even-Q):
            // The column (Q) is mapped directly.
            // The row is adjusted based on the column parity.
            // Staggering ensures that hex centers align on a grid while maintaining 6 neighbors.
            int col = cube.Q;
            int row = cube.R + (cube.Q + (useEvenStagger ? (cube.Q & 1) : -(cube.Q & 1))) / 2;
            return (col, row);
        }
    }

    /// <summary>
    /// Converts offset coordinates (column, row) to cube coordinates.
    /// </summary>
    /// <param name="col">The column coordinate.</param>
    /// <param name="row">The row coordinate.</param>
    /// <param name="orientation">The top orientation of the hex grid.</param>
    /// <param name="useEvenStagger">True if even columns/rows are shifted down (Flat/Pointy).</param>
    /// <returns>The equivalent cube coordinate.</returns>
    public static CubeCoordinate OffsetToCube(int col, int row, HexTopOrientation orientation = HexTopOrientation.PointyTopped, bool useEvenStagger = false)
    {
        if (orientation == HexTopOrientation.PointyTopped)
        {
            int q = col - (row + (useEvenStagger ? (row & 1) : -(row & 1))) / 2;
            int r = row;
            return new CubeCoordinate(q, r, -q - r);
        }
        else
        {
            int q = col;
            int r = row - (col + (useEvenStagger ? (col & 1) : -(col & 1))) / 2;
            return new CubeCoordinate(q, r, -q - r);
        }
    }

    /// <summary>
    /// Calculates all hexes that a line segment between two coordinates passes through.
    /// This implementation uses high-density sampling to ensure that hexes "touched" 
    /// by the line (even if just on an edge) are captured.
    /// </summary>
    public static List<CubeCoordinate> GetLine(CubeCoordinate a, CubeCoordinate b)
    {
        int dist = a.DistanceTo(b);
        if (dist == 0) return new List<CubeCoordinate> { a };

        var results = new HashSet<CubeCoordinate>();
        
        // Sample densely (3x per hex distance) to catch all hexes the line might "touch".
        int samples = dist * 3;
        for (int i = 0; i <= samples; i++)
        {
            double t = (double)i / samples;
            var frac = a.Lerp(b, t);
            
            // Round to the nearest hex
            results.Add(frac.Round());
            
            // Support showing "both" hexes if we are very close to an edge.
            // We do this by slightly nudging the fractional coordinate and rounding again.
            // This captures hexes "touched" by a zero-width line.
            foreach (var nearHex in GetNearHexes(frac))
            {
                results.Add(nearHex);
            }
        }
        
        return results.ToList();
    }

    private static IEnumerable<CubeCoordinate> GetNearHexes(FractionalHex h)
    {
        // A very small epsilon 
        const double eps = 1e-5;
        
        // Nudge in all 6 principal directions to see if we cross into a neighbor
        // within the "touch" threshold.
        double[] dq = { eps, -eps, 0, 0, eps, -eps };
        double[] dr = { 0, 0, eps, -eps, -eps, eps };
        
        for (int i = 0; i < 6; i++)
        {
            var nudged = new FractionalHex(h.Q + dq[i], h.R + dr[i], h.S - dq[i] - dr[i]);
            yield return nudged.Round();
        }
    }
}
