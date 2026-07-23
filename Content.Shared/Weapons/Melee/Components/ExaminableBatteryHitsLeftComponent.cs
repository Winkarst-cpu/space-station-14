using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Melee.Components;

/// <summary>
/// With this component, examining the item will show how many more times it can hit things before the battery is depleted.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ExaminableBatteryHitsLeftSystem))]
public sealed partial class ExaminableBatteryHitsLeftComponent : Component
{
    /// <summary>
    /// The text that will be shown on examine.
    /// </summary>
    [DataField]
    public LocId ExamineText = "examine-battery-hits-left";

    /// <summary>
    /// The color of the use count.
    /// </summary>
    [DataField]
    public Color Color = Color.Yellow;
}
