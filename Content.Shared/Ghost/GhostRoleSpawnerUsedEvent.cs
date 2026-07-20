namespace Content.Shared.Ghost;

/// <summary>
/// Raised on a spawned entity after they use a ghost role mob spawner.
/// </summary>
[ByRefEvent]
public record struct GhostRoleSpawnerUsedEvent(EntityUid Spawner, EntityUid Spawned);
