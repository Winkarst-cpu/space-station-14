using Content.Shared.Destructible;
using Content.Shared.EntityTable;
using Content.Shared.Gatherable.Components;
using Content.Shared.Interaction;
using Content.Shared.Projectiles;
using Content.Shared.Random.Helpers;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Shared.Gatherable;

public sealed partial class GatherableSystem : EntitySystem
{
    [Dependency] private SharedDestructibleSystem _destructible = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private TagSystem _tagSystem = default!;
    [Dependency] private EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private EntityTableSystem _entityTable = default!;
    [Dependency] private IGameTiming _timing = default!;

    [SubscribeLocalEvent]
    private void OnAttacked(Entity<GatherableComponent> gatherable, ref AttackedEvent args)
    {
        if (_whitelistSystem.IsWhitelistFailOrNull(gatherable.Comp.ToolWhitelist, args.Used))
            return;

        Gather(gatherable.AsNullable(), args.User);
    }

    [SubscribeLocalEvent]
    private void OnActivate(Entity<GatherableComponent> gatherable, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (_whitelistSystem.IsWhitelistFailOrNull(gatherable.Comp.ToolWhitelist, args.User))
            return;

        Gather(gatherable.AsNullable(), args.User);
        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnProjectileCollide(Entity<GatheringProjectileComponent> gathering, ref StartCollideEvent args)
    {
        if (!args.OtherFixture.Hard)
            return;

        if (args.OurFixtureId != SharedProjectileSystem.ProjectileFixture)
            return;

        if (gathering.Comp.Amount <= 0)
            return;

        if (!TryComp<GatherableComponent>(args.OtherEntity, out var gatherable))
            return;

        Gather((args.OtherEntity, gatherable), gathering);
        gathering.Comp.Amount--;
        Dirty(gathering);

        if (gathering.Comp.Amount <= 0)
            PredictedQueueDel(gathering);
    }

    /// <summary>
    /// Destroys the gathered entity, plays a sound if it has <see cref="SoundOnGatherComponent"/>, and spawns loot if possible.
    /// </summary>
    /// <param name="gathered">The entity that was gathered.</param>
    /// <param name="gatherer">The entity that gathered it.</param>
    public void Gather(Entity<GatherableComponent?> gathered, EntityUid? gatherer = null)
    {
        if (!Resolve(gathered, ref gathered.Comp))
            return;

        var pos = Transform(gathered).Coordinates;
        if (TryComp<SoundOnGatherComponent>(gathered, out var soundComp))
            _audio.PlayPredicted(soundComp.Sound, pos, gatherer);

        _destructible.DestroyEntity(gathered);

        if (gathered.Comp.Loot == null)
            return;

        foreach (var (tag, table) in gathered.Comp.Loot)
        {
            if (tag != "All" && gatherer != null && !_tagSystem.HasTag(gatherer.Value, tag))
                continue;

            var random = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(gathered));
            foreach (var loot in _entityTable.GetSpawns(table, random))
            {
                PredictedSpawnAtPosition(loot, pos.Offset(random.NextVector2(gathered.Comp.GatherOffset)));
            }
        }
    }
}
