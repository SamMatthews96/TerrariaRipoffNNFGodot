using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class ActionController : Node {
    [Export] private Player _player;
    private PlayerAction _currentPlayerAction;

    [Export] public GatherAction GatherAction { get; private set; }
    [Export] public BuildAction BuildAction { get; private set; }

    private Game _game;
    public event Action<PlayerActionType> ActionChanged;

    public void InitAsLocal(Game game) {
        _game = game;
        _game.InputManager.LeftMouseUp += OnInputManagerLeftMouseUp;
        _game.InputManager.LeftMouseDown += OnInputManagerLeftMouseDown;
        _game.InputManager.RightMouseUp += OnInputManagerRightMouseUp;
        _game.InputManager.RightMouseDown += OnInputManagerRightMouseDown;
        _game.Interface.ActionBar.ButtonClicked += EquipAction;
        _game.InputManager.PlayerActionModeChanged += EquipAction;
        TreeExiting += OnTreeExitingLocal;
        EquipAction(PlayerActionType.Gather);
        
        BuildAction.InitAsLocal(game);
        GatherAction.InitAsLocal(game);
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
        BuildAction.InitAsHost(game);
        TreeExiting += OnTreeExitingHost;
    }

    private void OnTreeExitingHost() {
        TreeExiting -= OnTreeExitingHost;
    }

    private void OnInputManagerLeftMouseUp(Vector2 mouseWorldPosition) {
        _currentPlayerAction.EndLeftMouseAction(mouseWorldPosition);
    }

    private void OnInputManagerLeftMouseDown(Vector2 mouseWorldPosition) {
        _currentPlayerAction.LeftMouseAction(mouseWorldPosition);
    }

    private void OnInputManagerRightMouseDown(Vector2 mouseWorldPosition) {
        _currentPlayerAction.RightMouseAction(mouseWorldPosition);
    }

    private void OnInputManagerRightMouseUp(Vector2 mouseWorldPosition) {
        _currentPlayerAction.EndRightMouseAction(mouseWorldPosition);
    } 

    private void EquipAction(PlayerActionType state) {
        _currentPlayerAction = state switch {
            PlayerActionType.Gather => GatherAction,
            PlayerActionType.Build => BuildAction,
            // PlayerActionType.Weapon
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

        ActionChanged?.Invoke(state);
    }
}