﻿using Content.Shared.Beeper.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Item.ItemToggle;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared.Beeper.Systems;

/// <summary>
/// This handles generic proximity beeper logic.
/// </summary>
public sealed class BeeperSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BeeperComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BeeperComponent> ent, ref MapInitEvent args)
    {
        SetIntervalScaling((ent, ent.Comp), ent.Comp.IntervalScaling);

        ent.Comp.NextBeep = _timing.CurTime + ent.Comp.Interval;
        DirtyField(ent, ent.Comp, nameof(ent.Comp.NextBeep));
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<BeeperComponent>();

        while (query.MoveNext(out var uid, out var beeper))
        {
            if (beeper.NextBeep > _timing.CurTime)
                continue;

            beeper.NextBeep += beeper.Interval;
            DirtyField(uid, beeper, nameof(beeper.NextBeep));

            if (!_toggle.IsActivated(uid))
                continue;

            var beepEvent = new BeepPlayedEvent(beeper.IsMuted);
            RaiseLocalEvent(uid, ref beepEvent);

            if (!beeper.IsMuted)
                _audio.PlayPredicted(beeper.BeepSound, uid, null);
        }
    }

    /// <summary>
    /// Sets beeper interval scaling. The higher the value, the more frequent beeper will beep.
    /// </summary>
    public void SetIntervalScaling(Entity<BeeperComponent?> ent, FixedPoint2 newScaling)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        newScaling = FixedPoint2.Clamp(newScaling, 0, 1);

        if (ent.Comp.IntervalScaling == newScaling)
            return;

        ent.Comp.IntervalScaling = FixedPoint2.Clamp(newScaling, 0, 1);
        DirtyField(ent, ent.Comp, nameof(ent.Comp.IntervalScaling));

        ent.Comp.Interval = (ent.Comp.MaxBeepInterval - ent.Comp.MinBeepInterval) * ent.Comp.IntervalScaling.Float() + ent.Comp.MinBeepInterval;
        DirtyField(ent, ent.Comp, nameof(ent.Comp.Interval));
    }

    /// <summary>
    /// Sets whether or not beeper should be muted.
    /// </summary>
    public void SetMute(Entity<BeeperComponent?> ent, bool isMuted)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        ent.Comp.IsMuted = isMuted;
        DirtyField(ent, ent.Comp, nameof(ent.Comp.IsMuted));
    }
}
