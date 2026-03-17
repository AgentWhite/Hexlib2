namespace HexLib;

/// <summary>
/// Represents the supported physical orientations of a board in 90-degree increments.
/// </summary>
public enum BoardOrientation
{
    /// <summary>Standard, unrotated alignment.</summary>
    Degree0 = 0,
    
    /// <summary>Rotated 90 degrees clockwise.</summary>
    Degree90 = 90,
    
    /// <summary>Rotated 180 degrees (upside down).</summary>
    Degree180 = 180,
    
    /// <summary>Rotated 270 degrees clockwise (or 90 counter-clockwise).</summary>
    Degree270 = 270
}
