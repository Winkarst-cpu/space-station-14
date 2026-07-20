using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Bible.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Timing;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared.Bible;

/// <summary>
/// Logic for the Bible components.
/// </summary>
public abstract partial class SharedBibleSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private DamageableSystem _damageableSystem = default!;
    [Dependency] private InventorySystem _invSystem = default!;
    [Dependency] private MobStateSystem _mobStateSystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private SharedActionsSystem _actionsSystem = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private UseDelaySystem _delay = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private IGameTiming _timing = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BibleComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<SummonableComponent, GetVerbsEvent<AlternativeVerb>>(AddSummonVerb);
        SubscribeLocalEvent<SummonableComponent, GetItemActionsEvent>(GetSummonAction);
        SubscribeLocalEvent<SummonableComponent, SummonActionEvent>(OnSummon);
        SubscribeLocalEvent<FamiliarComponent, MobStateChangedEvent>(OnFamiliarDeath);
    }

    /// <summary>
    /// This handles familiar respawning.
    /// </summary>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SummonableRespawningComponent, SummonableComponent>();
        while (query.MoveNext(out var uid, out var respawningComponent, out var summonableComp))
        {
            if (summonableComp.RespawnTime > _timing.CurTime)
                continue;

            // Delete old summoned entity.
            if (summonableComp.SummonedEntity.HasValue)
            {
                PredictedDel(summonableComp.SummonedEntity);
                summonableComp.SummonedEntity = null;
            }

            summonableComp.CanSummon = true;
            Dirty(uid, summonableComp);

            if (_timing.IsFirstTimePredicted)
            {
                _popupSystem.PopupEntity(Loc.GetString(summonableComp.LocPrefix + "-summon-respawn-ready", ("book", uid)), uid, PopupType.Medium);
                _audio.PlayPvs(summonableComp.SummonSound, uid);
            }

            RemComp(uid, respawningComponent);
        }
    }

    private void OnAfterInteract(Entity<BibleComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        if (!TryComp(ent, out UseDelayComponent? useDelay) || _delay.IsDelayed((ent, useDelay)))
            return;

        if (args.Target == null || args.Target == args.User || !_mobStateSystem.IsAlive(args.Target.Value))
            return;

        // Sizzle user if they are not a Bible user.
        if (!HasComp<BibleUserComponent>(args.User))
        {
            _popupSystem.PopupEntity(Loc.GetString(ent.Comp.LocPrefix + "-sizzle"), args.User, args.User);

            _audio.PlayPredicted(ent.Comp.SizzleSound, ent, args.User);
            _damageableSystem.TryChangeDamage(args.User, ent.Comp.DamageOnUntrainedUse, true, origin: ent);
            _delay.TryResetDelay((ent, useDelay));

            return;
        }

        var userEnt = Identity.Entity(args.User, EntityManager);
        var targetEnt = Identity.Entity(args.Target.Value, EntityManager);

        // This only has a chance to fail if the target is not wearing anything on their head and is not a familiar.
        if (!_invSystem.TryGetSlotEntity(args.Target.Value, "head", out _) && !HasComp<FamiliarComponent>(args.Target.Value))
        {
            var rand = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(ent));
            if (rand.Prob(ent.Comp.FailChance))
            {
                var othersFailMessage = Loc.GetString(ent.Comp.LocPrefix + "-heal-fail-others", ("user", userEnt), ("target", targetEnt), ("bible", ent));
                _popupSystem.PopupEntity(othersFailMessage, args.User, Filter.PvsExcept(args.User), true, PopupType.SmallCaution);

                var selfFailMessage = Loc.GetString(ent.Comp.LocPrefix + "-heal-fail-self", ("target", targetEnt), ("bible", ent));
                _popupSystem.PopupEntity(selfFailMessage, args.User, args.User, PopupType.MediumCaution);

                _audio.PlayPredicted(ent.Comp.BibleHitSound, ent, args.User);
                _damageableSystem.TryChangeDamage(args.Target.Value, ent.Comp.DamageOnFail, true, origin: ent);
                _delay.TryResetDelay((ent, useDelay));

                return;
            }
        }

        string othersMessage;
        string selfMessage;

        if (_damageableSystem.TryChangeDamage(args.Target.Value, ent.Comp.Damage, true, origin: ent))
        {
            othersMessage = Loc.GetString(ent.Comp.LocPrefix + "-heal-success-others", ("user", userEnt), ("target", targetEnt), ("bible", ent));
            selfMessage = Loc.GetString(ent.Comp.LocPrefix + "-heal-success-self", ("target", targetEnt), ("bible", ent));

            _audio.PlayPredicted(ent.Comp.HealSound, ent, args.User);
            _delay.TryResetDelay((ent, useDelay));

            if (ent.Comp.HealingLightEffect.HasValue)
                Spawn(ent.Comp.HealingLightEffect.Value, new EntityCoordinates(args.Target.Value, default));
        }
        else
        {
            othersMessage = Loc.GetString(ent.Comp.LocPrefix + "-heal-success-none-others", ("user", userEnt), ("target", targetEnt), ("bible", ent));
            selfMessage = Loc.GetString(ent.Comp.LocPrefix + "-heal-success-none-self", ("target", targetEnt), ("bible", ent));
        }

        _popupSystem.PopupEntity(othersMessage, args.User, Filter.PvsExcept(args.User), true, PopupType.Medium);
        _popupSystem.PopupEntity(selfMessage, args.User, args.User, PopupType.Large);
    }

    private void AddSummonVerb(Entity<SummonableComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || !ent.Comp.CanSummon || !ent.Comp.SummonEntityPrototype.HasValue)
            return;

        var user = args.User;
        if (ent.Comp.RequiresBibleUser && !HasComp<BibleUserComponent>(user))
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                AttemptSummon(ent, user);
            },
            Text = Loc.GetString(ent.Comp.LocPrefix + "-summon-verb"),
            Priority = 2
        };
        args.Verbs.Add(verb);
    }

    private void GetSummonAction(Entity<SummonableComponent> ent, ref GetItemActionsEvent args)
    {
        if (!ent.Comp.CanSummon)
            return;

        args.AddAction(ref ent.Comp.SummonActionEntity, ent.Comp.SummonActionPrototype);
    }

    private void OnSummon(Entity<SummonableComponent> ent, ref SummonActionEvent args)
    {
        AttemptSummon(ent, args.Performer);
    }

    private void OnFamiliarDeath(Entity<FamiliarComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        StartRespawnTimer(ent);
    }

    /// <summary>
    /// Starts up the respawn timer for the chaplain's familiar.
    /// </summary>
    protected void StartRespawnTimer(Entity<FamiliarComponent> ent)
    {
        if (!TryComp<SummonableComponent>(ent.Comp.Source, out var summonable))
            return;

        AddComp<SummonableRespawningComponent>(ent.Comp.Source.Value);

        summonable.RespawnTime = _timing.CurTime + summonable.RespawnCooldown;
        Dirty(ent.Comp.Source.Value, summonable);
    }

    /// <summary>
    /// Attempts to summon a new familiar.
    /// </summary>
    private void AttemptSummon(Entity<SummonableComponent> ent, Entity<TransformComponent?> user)
    {
        if (!Resolve(user, ref user.Comp))
            return;

        if (!ent.Comp.CanSummon || !ent.Comp.SummonEntityPrototype.HasValue)
            return;

        if (ent.Comp.RequiresBibleUser && !HasComp<BibleUserComponent>(user))
            return;

        if (!_blocker.CanInteract(user, ent))
            return;

        // Make this familiar the component's summon
        var familiar = PredictedSpawnAtPosition(ent.Comp.SummonEntityPrototype, user.Comp.Coordinates);
        ent.Comp.SummonedEntity = familiar;

        // If this is going to use a ghost role mob spawner, attach it to the bible.
        if (HasComp<GhostRoleMobSpawnerComponent>(familiar))
        {
            _popupSystem.PopupEntity(Loc.GetString(ent.Comp.LocPrefix + "-summon-requested"), user, user, PopupType.Medium);
            _transform.SetParent(familiar, ent);
        }

        EnsureComp<FamiliarComponent>(familiar, out var familiarComponent);
        familiarComponent.Source = ent;
        Dirty(familiar, familiarComponent);

        _actionsSystem.RemoveAction(user.Owner, ent.Comp.SummonActionEntity);

        ent.Comp.CanSummon = false;
        Dirty(ent);
    }
}
