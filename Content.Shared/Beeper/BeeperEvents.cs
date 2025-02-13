namespace Content.Shared.Beeper;

/// <summary>
/// Raised after entity beeped.
/// </summary>
[ByRefEvent]
public readonly record struct BeepPlayedEvent(bool Muted)
{
}
