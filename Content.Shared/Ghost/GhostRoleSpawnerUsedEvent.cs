namespace Content.Shared.Ghost;

/// <summary>
/// Raised on a spawned entity after they use a ghost role mob spawner.
/// </summary>
public sealed class GhostRoleSpawnerUsedEvent(EntityUid spawner, EntityUid spawned) : EntityEventArgs
{
    /// <summary>
    /// The entity that spawned this.
    /// </summary>
    public EntityUid Spawner = spawner;

    /// <summary>
    /// The entity spawned.
    /// </summary>
    public EntityUid Spawned = spawned;
}
