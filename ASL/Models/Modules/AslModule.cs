using System.Collections.Generic;

namespace ASL.Models.Modules;

/// <summary>
/// Represents a physical ASL game module and its properties.
/// </summary>
public class AslModule
{
    /// <summary>
    /// Gets or sets the file path to the front image of the module box.
    /// </summary>
    public string? FrontImage { get; set; }

    /// <summary>
    /// Gets or sets the file path to the back image of the module box.
    /// </summary>
    public string? BackImage { get; set; }

    /// <summary>
    /// Gets or sets the specific module identity.
    /// </summary>
    public Module Module { get; set; }

    /// <summary>
    /// Gets or sets the full display name of the module.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the module contents.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of other modules required to use this module.
    /// </summary>
    public List<AslModule> RequiredModules { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether all data for this module has been fully implemented.
    /// </summary>
    public bool IsFinished { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AslModule"/> class.
    /// </summary>
    public AslModule() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AslModule"/> class with basic info.
    /// </summary>
    /// <param name="module">The module identity.</param>
    /// <param name="fullName">Grand full name.</param>
    /// <param name="description">Module overview.</param>
    public AslModule(Module module, string fullName, string description = "")
    {
        Module = module;
        FullName = fullName;
        Description = description;
    }
}
