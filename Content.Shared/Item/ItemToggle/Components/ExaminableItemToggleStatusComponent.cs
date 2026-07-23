using Robust.Shared.GameStates;

namespace Content.Shared.Item.ItemToggle.Components;

/// <summary>
/// With this component, <see cref="ItemToggleComponent"/> will show its status on examine.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(ItemToggleSystem))]
public sealed partial class ExaminableItemToggleStatusComponent : Component
{
    /// <summary>
    /// The text to show if the item is toggled on.
    /// </summary>
    [DataField(required: true)]
    public LocId OnText;

    /// <summary>
    /// The text to show if the item is toggled off.
    /// </summary>
    [DataField(required: true)]
    public LocId OffText;
}
