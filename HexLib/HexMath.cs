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
            int col = cube.Q + (cube.R + (useEvenStagger ? (cube.R & 1) : -(cube.R & 1))) / 2;
            int row = cube.R;
            return (col, row);
        }
        else
        {
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
}
