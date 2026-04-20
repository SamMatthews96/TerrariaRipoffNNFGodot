using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class ActionController : Node {
    private PlayerAction _currentPlayerAction;
    public Player Player { get; private set; }
    public Game Game { get; private set; }

    [Export] public GatherAction GatherAction { get; private set; }
    [Export] public BuildAction BuildAction { get; private set; }
    [Export] public WeaponAction WeaponAction { get; private set; }
    
    public event Action<PlayerActionType> ActionChanged;

    public static ActionController Create(Game game, Player player) {
        ActionController newController = Data.PackedScenes.PlayerActionController
            .Instantiate<ActionController>();
        newController.Game = game;
        newController.Player = player;

        return newController;
    }

    public override void _Ready() {
        Game.World.InputManager.LeftMouseUp += OnInputManagerLeftMouseUp;
        Game.World.InputManager.LeftMouseDown += OnInputManagerLeftMouseDown;
        Game.World.InputManager.RightMouseUp += OnInputManagerRightMouseUp;
        Game.World.InputManager.RightMouseDown += OnInputManagerRightMouseDown;
        Game.World.InputManager.PlayerActionModeChanged += EquipAction;
        Game.World.Interface.ActionBar.ButtonClicked += EquipAction;
        TreeExiting += OnTreeExiting;

        EquipAction(PlayerActionType.Gather);
    }

    private void OnTreeExiting() {
        Game.World.InputManager.LeftMouseUp -= OnInputManagerLeftMouseUp;
        Game.World.InputManager.LeftMouseDown -= OnInputManagerLeftMouseDown;
        Game.World.InputManager.RightMouseUp -= OnInputManagerRightMouseUp;
        Game.World.InputManager.RightMouseDown -= OnInputManagerRightMouseDown;
        Game.World.InputManager.PlayerActionModeChanged -= EquipAction;
        Game.World.Interface.ActionBar.ButtonClicked -= EquipAction;
        TreeExiting -= OnTreeExiting;
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
            PlayerActionType.Weapon => WeaponAction,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

        ActionChanged?.Invoke(state);
    }
}