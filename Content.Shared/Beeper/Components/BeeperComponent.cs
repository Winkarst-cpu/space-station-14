using Content.Shared.Beeper.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Beeper.Components;

/// <summary>
/// Used to make entity beep.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause, Access(typeof(BeeperSystem))]
public sealed partial class BeeperComponent : Component
{
    /// <summary>
    /// How much to scale the interval by (1 - max, 0 - min).
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 IntervalScaling = 0;

    /// <summary>
    /// The maximum interval between beeps.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan MaxBeepInterval = TimeSpan.FromSeconds(1.5f);

    /// <summary>
    /// The minimum interval between beeps.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan MinBeepInterval = TimeSpan.FromSeconds(0.25f);

    /// <summary>
    /// Interval for the next beep.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan Interval;

    /// <summary>
    /// Next time entity beeps.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextBeep = TimeSpan.Zero;

    /// <summary>
    /// Is the beep muted.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsMuted;

    /// <summary>
    /// The sound played when the entity beeps.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? BeepSound;
}
