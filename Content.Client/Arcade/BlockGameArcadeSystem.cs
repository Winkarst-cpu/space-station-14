using Content.Client.Arcade.UI;
using Content.Shared.Arcade.Components;
using Content.Shared.Arcade.Enums;
using Content.Shared.Arcade.Systems;

namespace Content.Client.Arcade;

/// <inheritdoc/>
public sealed partial class BlockGameArcadeSystem : SharedBlockGameArcadeSystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    protected override void CreateUIGrid(Entity<BlockGameArcadeComponent> ent)
    {
        if (_ui.TryGetOpenUi<BlockGameArcadeBoundUserInterface>(ent.Owner, ArcadeUiKey.Key, out var bui))
        {
            bui.CreateGrid(ent.Comp.GridSize.X, ent.Comp.Grid);
        }
    }

    protected override void UpdateUIGridCell(Entity<BlockGameArcadeComponent> ent, int index, Color newColor)
    {
        if (_ui.TryGetOpenUi<BlockGameArcadeBoundUserInterface>(ent.Owner, ArcadeUiKey.Key, out var bui))
        {
            bui.UpdateGridCell(index, newColor);
        }
    }
}
