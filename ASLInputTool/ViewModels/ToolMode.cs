namespace ASLInputTool.ViewModels;

/// <summary>
/// Specifies the active tool mode for the board editor.
/// </summary>
public enum ToolMode
{
    /// <summary>Select hexes to view/edit properties.</summary>
    Select,
    /// <summary>Paint terrain directly onto hexes.</summary>
    Paint,
    /// <summary>Zoom in on the board (sticky tool).</summary>
    ZoomIn,
    /// <summary>Zoom out from the board (sticky tool).</summary>
    ZoomOut,
    /// <summary>Create a string of road/path hexsides.</summary>
    Road
}
