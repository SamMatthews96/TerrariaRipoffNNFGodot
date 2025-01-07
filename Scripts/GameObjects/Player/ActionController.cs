using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class ActionController : Node {
    [Export] private Player _player;
    private PlayerAction _currentPlayerAction;

    [Export] private GatherAction _gatherAction;
    [Export] private BuildAction _buildAction;

    private Game _game;
    public event Action<PlayerAction.Type> ActionChanged;
    public event Action<IntVector, float> GatherAttempted;
    public event Action<Item, IntVector> BlockPlaced;

    public void InitAsLocal(Game game) {
        _game = game;
        _game.InputManager.LeftMouseUp += OnInputManagerLeftMouseUp;
        _game.InputManager.LeftMouseDown += OnInputManagerLeftMouseDown;
        _game.Interface.ActionBar.ButtonClicked += EquipAction;
        _game.InputManager.PlayerActionModeChanged += EquipAction;
        TreeExiting += OnTreeExitingLocal;
        EquipAction(PlayerAction.Type.Gather);
        
        _buildAction.InitAsLocal(game);
        _gatherAction.InitAsLocal(game);
    }

    private void OnTreeExitingLocal() {
        _game.InputManager.LeftMouseUp -= OnInputManagerLeftMouseUp;
        _game.InputManager.LeftMouseDown -= OnInputManagerLeftMouseDown;
        _game.Interface.ActionBar.ButtonClicked -= EquipAction;
        _game.InputManager.PlayerActionModeChanged -= EquipAction;
        TreeExiting -= OnTreeExitingLocal;
    }

    public void InitAsHost(Game game) {
        _game = game;
        // _gatherAction.InitAsHost(game);
        _buildAction.InitAsHost(game);
        _gatherAction.GatherAttempted += OnGatherAttempted;
        _buildAction.BlockPlaced += OnBlockPlaced;
        TreeExiting += OnTreeExitingHost;
    }

    private void OnTreeExitingHost() {
        _gatherAction.GatherAttempted -= OnGatherAttempted;
        _buildAction.BlockPlaced -= OnBlockPlaced;
        TreeExiting -= OnTreeExitingHost;
    }

    private void OnInputManagerLeftMouseUp(Vector2 mouseWorldPosition) {
        _currentPlayerAction.EndPrimaryAction(mouseWorldPosition);
    }

    private void OnInputManagerLeftMouseDown(Vector2 mouseWorldPosition) {
        _currentPlayerAction.PrimaryAction(mouseWorldPosition);
    }

    private void EquipAction(PlayerAction.Type state) {
        _currentPlayerAction = state switch {
            PlayerAction.Type.Gather => _gatherAction,
            PlayerAction.Type.Build => _buildAction,
            PlayerAction.Type.Weapon => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

        ActionChanged?.Invoke(state);
    }

    private void OnGatherAttempted(IntVector coords, float damage) {
        GatherAttempted?.Invoke(coords, damage);
    }

    private void OnBlockPlaced(Item blockType, IntVector coords) {
        BlockPlaced?.Invoke(blockType, coords);
    }
}