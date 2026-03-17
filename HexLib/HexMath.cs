namespace HexLib;

// Helper to handle coordinate conversions for pointy-topped hexes with odd-r layout
// This is typically the standard for wargames (columns are straight, rows zigzag).
public static class HexMath
{
    // Converts cube coordinates to offset coordinates (column, row) assuming "odd-r" pointy-top layout
    public static (int col, int row) CubeToOffset(CubeCoordinate cube)
    {
        int col = cube.Q + (cube.R - (cube.R & 1)) / 2;
        int row = cube.R;
        return (col, row);
    }

    // Converts offset coordinates (column, row) to cube coordinates assuming "odd-r" pointy-top layout
    public static CubeCoordinate OffsetToCube(int col, int row)
    {
        int q = col - (row - (row & 1)) / 2;
        int r = row;
        int s = -q - r;
        return new CubeCoordinate(q, r, s);
    }
}
