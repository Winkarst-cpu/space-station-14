using Content.Shared.Examine;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Weapons.Melee.Components;

namespace Content.Shared.Weapons.Melee;

/// <summary>
/// This system handles showing how many more times a weapon can hit things before the battery is depleted.
/// </summary>
public sealed partial class ExaminableBatteryHitsLeftSystem : EntitySystem
{
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private SharedMeleeWeaponSystem _meleeWeapon = default!;

    [SubscribeLocalEvent]
    private void OnExamined(Entity<ExaminableBatteryHitsLeftComponent> ent, ref ExaminedEvent args)
    {
        var count = _battery.GetRemainingUses(ent.Owner, _meleeWeapon.GetHitPowerCost(ent));
        args.PushMarkup(Loc.GetString(ent.Comp.ExamineText, ("color", ent.Comp.Color), ("count", count)));
    }
}
