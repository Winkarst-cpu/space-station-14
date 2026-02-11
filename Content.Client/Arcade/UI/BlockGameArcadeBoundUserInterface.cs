using Content.Shared.Arcade.Components;
using Content.Shared.Arcade.Enums;
using Content.Shared.Arcade.Messages;
using Content.Shared.Arcade.Messages.BlockGame;
using Robust.Client.Player;
using Robust.Client.UserInterface;

namespace Content.Client.Arcade.UI;

/// <summary>
///
/// </summary>
public sealed class BlockGameArcadeBoundUserInterface(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private BlockGameArcadeWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<BlockGameArcadeWindow>();
        _window.Title = EntMan.GetComponent<MetaDataComponent>(Owner).EntityName;
        _window.NewGameButton.OnPressed += _ => SendPredictedMessage(new ArcadeNewGameMessage());

        _window.OnAction += OnAction;

        if (EntMan.TryGetComponent<BlockGameArcadeComponent>(Owner, out var blockGame))
            _window.CreateGrid(blockGame.GridSize.X, blockGame.Grid);

        if (EntMan.TryGetComponent<ArcadeComponent>(Owner, out var arcade) && arcade.Player != _playerManager.LocalEntity)
            _window.SetUsability(false);

        _window.OpenCentered();
    }

    private void OnAction(BlockGameArcadeAction action)
    {
        switch (action)
        {
            case BlockGameArcadeAction.Down:
                SendPredictedMessage(new BlockGameArcadeActionDownMessage());
                break;
            case BlockGameArcadeAction.Left:
                SendPredictedMessage(new BlockGameArcadeActionLeftMessage());
                break;
            case BlockGameArcadeAction.Right:
                SendPredictedMessage(new BlockGameArcadeActionRightMessage());
                break;
            case BlockGameArcadeAction.Drop:
                SendPredictedMessage(new BlockGameArcadeActionDropMessage());
                break;
            case BlockGameArcadeAction.Rotate:
                SendPredictedMessage(new BlockGameArcadeActionRotateMessage());
                break;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public void CreateGrid(int gridWidth, Color[] grid)
    {
        _window?.CreateGrid(gridWidth, grid);
    }

    /// <summary>
    ///
    /// </summary>
    public void UpdateGridCell(int index, Color newColor)
    {
        _window?.UpdateGridCell(index, newColor);
    }
}
