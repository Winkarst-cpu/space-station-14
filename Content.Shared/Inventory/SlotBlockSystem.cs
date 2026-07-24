using Content.Shared.Inventory.Events;

namespace Content.Shared.Inventory;

/// <summary>
/// Handles prevention of items being unequipped and equipped from slots that are blocked by <see cref="SlotBlockComponent"/>.
/// </summary>
public abstract partial class SlotBlockSystem : EntitySystem
{
    [SubscribeLocalEvent]
    private static void OnEquipAttempt(Entity<SlotBlockComponent> ent, ref InventoryRelayedEvent<IsEquippingTargetAttemptEvent> args)
    {
        if (args.Args.Cancelled)
            return;

        if ((args.Args.SlotFlags & ent.Comp.Slots) == 0)
            return;

        args.Args.Reason = "slot-block-component-blocked";
        args.Args.Cancel();
    }

    [SubscribeLocalEvent]
    private static void OnUnequipAttempt(Entity<SlotBlockComponent> ent, ref InventoryRelayedEvent<IsUnequippingTargetAttemptEvent> args)
    {
        if (args.Args.Cancelled)
            return;

        if ((args.Args.SlotFlags & ent.Comp.Slots) == 0)
            return;

        args.Args.Reason = "slot-block-component-blocked";
        args.Args.Cancel();
    }
}
