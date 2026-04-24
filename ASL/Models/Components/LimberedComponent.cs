using ASL.Core;
using ASL.Models.Units;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component marking an ordnance piece as capable of being limbered for tow. Describes capability only;
/// the current limbered/unlimbered state belongs on the runtime unit state layer, not here.
/// </summary>
public class LimberedComponent : IUnitComponent
{
    /// <inheritdoc />
    [JsonIgnore]
    public Unit? Owner { get; set; }

    /// <inheritdoc />
    public string ComponentName => GetType().Name;

    /// <inheritdoc />
    public void Initialize(Unit owner) => Owner = owner;

    /// <summary>Gets or sets the Portage Point cost of the piece when limbered for tow.</summary>
    public int LimberedPortageCost { get; set; }
}
