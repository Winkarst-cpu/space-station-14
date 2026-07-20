using Content.Server.Ghost.Roles.Events;
using Content.Shared.Bible;
using Content.Shared.Bible.Components;

namespace Content.Server.Bible;

/// <inheritdoc/>
public sealed class BibleSystem : SharedBibleSystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FamiliarComponent, ComponentRemove>(OnFamiliarRemoved);
        SubscribeLocalEvent<FamiliarComponent, GhostRoleSpawnerUsedEvent>(OnSpawned);
    }

    private void OnFamiliarRemoved(Entity<FamiliarComponent> ent, ref ComponentRemove args)
    {
        StartRespawnTimer(ent);
    }

    /// <summary>
    /// When the familiar spawns, set its source to the bible.
    /// </summary>
    private void OnSpawned(Entity<FamiliarComponent> ent, ref GhostRoleSpawnerUsedEvent args)
    {
        var parent = Transform(args.Spawner).ParentUid;
        if (!TryComp<SummonableComponent>(parent, out var summonable))
            return;

        ent.Comp.Source = parent;
        Dirty(ent);

        summonable.SummonedEntity = ent;
        Dirty(parent, summonable);
    }
}
