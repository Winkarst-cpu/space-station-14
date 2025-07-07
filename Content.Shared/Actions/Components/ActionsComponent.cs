using Robust.Shared.GameStates;

namespace Content.Shared.Actions.Components;

/// <summary>
/// Lets the player controlling this entity use actions.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, Access(typeof(SharedActionsSystem))]
public sealed partial class ActionsComponent : Component
{
    /// <summary>
    /// List of actions currently granted to this entity.
    /// On the client, this may contain a mixture of client-side and networked entities.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Actions = new();
}

/// <summary>
///     Determines how the action icon appears in the hotbar for item actions.
/// </summary>
public enum ItemActionIconStyle : byte
{
    /// <summary>
    /// The default - The item icon will be big with a small action icon in the corner
    /// </summary>
    BigItem,

    /// <summary>
    /// The action icon will be big with a small item icon in the corner
    /// </summary>
    BigAction,

    /// <summary>
    /// BigAction but no item icon will be shown in the corner.
    /// </summary>
    NoItem
}
