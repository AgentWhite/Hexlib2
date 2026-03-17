namespace HexLib;

/// <summary>
/// Bitwise flags representing the physical edges of a rectangular map board that contain half-hexes,
/// intended to be joined directly to half-hexes on adjacent map boards.
/// </summary>
[Flags]
public enum BoardEdge
{
    None = 0,
    Top = 1,
    Right = 2,
    Bottom = 4,
    Left = 8
}
