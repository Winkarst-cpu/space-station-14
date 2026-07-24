using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Client.Inventory;

/// <inheritdoc/>
public sealed partial class ClientSlotBlockSystem : SlotBlockSystem
{
    [Dependency] private ClientInventorySystem _inventory = default!;

    [SubscribeLocalEvent]
    private void OnEquipped(Entity<SlotBlockComponent> ent, ref GotEquippedEvent args)
    {
        var enumerator = _inventory.GetSlotEnumerator(args.EquipTarget, ent.Comp.Slots);
        while (enumerator.MoveNext(out var container))
        {
            _inventory.AddSlotBlocker(args.EquipTarget, container.ID, ent);
        }
    }

    [SubscribeLocalEvent]
    private void OnUnequipped(Entity<SlotBlockComponent> ent, ref GotUnequippedEvent args)
    {
        var enumerator = _inventory.GetSlotEnumerator(args.EquipTarget, ent.Comp.Slots);
        while (enumerator.MoveNext(out var container))
        {
            _inventory.RemoveSlotBlocker(args.EquipTarget, container.ID, ent);
        }
    }
}
