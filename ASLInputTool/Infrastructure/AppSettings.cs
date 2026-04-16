namespace ASLInputTool.Infrastructure;

/// <summary>
/// Represents application-wide settings for the ASL Input Tool.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Gets or sets the path to the folder where board data is stored.
    /// </summary>
    public string BoardsFolder { get; set; } = string.Empty;
}
