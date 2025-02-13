using Content.Shared.Beeper.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Item.ItemToggle;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared.Beeper.Systems;

/// <summary>
/// Handles generic beeper logic
/// </summary>
public sealed class BeeperSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BeeperComponent, ComponentInit>(OnInit);
    }

    private void OnInit(Entity<BeeperComponent> ent, ref ComponentInit args)
    {
        var (_, component) = ent;

        UpdateBeepInterval(ent);

        component.NextBeep = _timing.CurTime + component.Interval;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<BeeperComponent>();

        while (query.MoveNext(out var uid, out var beeper))
        {
            if (beeper.NextBeep > _timing.CurTime)
                continue;

            beeper.NextBeep += beeper.Interval;
            Dirty(uid, beeper);

            if (!_toggle.IsActivated(uid))
                continue;

            var ev = new BeepPlayedEvent(beeper.IsMuted);
            RaiseLocalEvent(uid, ref ev);

            if (!beeper.IsMuted)
                _audio.PlayPredicted(beeper.BeepSound, uid, null);
        }
    }

    public void SetIntervalScaling(EntityUid uid, FixedPoint2 newScaling, BeeperComponent? beeper = null)
    {
        if (!Resolve(uid, ref beeper))
            return;

        beeper.IntervalScaling = FixedPoint2.Clamp(newScaling, 0, 1);
        Dirty(uid, beeper);

        UpdateBeepInterval((uid, beeper));
    }

    public void SetMute(EntityUid uid, bool isMuted, BeeperComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.IsMuted = isMuted;
        Dirty(uid, comp);
    }

    private void UpdateBeepInterval(Entity<BeeperComponent> ent)
    {
        var (uid, component) = ent;

        var scalingFactor = component.IntervalScaling.Float();
        var interval = (component.MaxBeepInterval - component.MinBeepInterval) * scalingFactor + component.MinBeepInterval;

        if (component.Interval == interval)
            return;

        component.Interval = interval;
        Dirty(uid, component);
    }
}
