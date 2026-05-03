using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class ActionController : Node {
    private PlayerAction _currentPlayerAction;
    [Export] public Player Player { get; private set; }
    [Export] public GatherAction Gather { get; private set; }
    [Export] public BuildAction Build { get; private set; }
    [Export] public WeaponAction WeaponAction { get; private set; }
    
    public event Action<PlayerActionType> ActionChanged;


    public override void _Ready() {
        if (!Player.IsLocalPlayer) return;
        Player.World.InputManager.LeftMouseUp += OnInputManagerLeftMouseUp;
        Player.World.InputManager.LeftMouseDown += OnInputManagerLeftMouseDown;
        Player.World.InputManager.RightMouseUp += OnInputManagerRightMouseUp;
        Player.World.InputManager.RightMouseDown += OnInputManagerRightMouseDown;
        Player.World.InputManager.PlayerActionModeChanged += EquipAction;
        Player.World.Interface.ActionBar.ButtonClicked += EquipAction;
        TreeExiting += OnTreeExiting;

        EquipAction(PlayerActionType.Gather);
    }

    private void OnTreeExiting() {
        Player.World.InputManager.LeftMouseUp -= OnInputManagerLeftMouseUp;
        Player.World.InputManager.LeftMouseDown -= OnInputManagerLeftMouseDown;
        Player.World.InputManager.RightMouseUp -= OnInputManagerRightMouseUp;
        Player.World.InputManager.RightMouseDown -= OnInputManagerRightMouseDown;
        Player.World.InputManager.PlayerActionModeChanged -= EquipAction;
        Player.World.Interface.ActionBar.ButtonClicked -= EquipAction;
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
            PlayerActionType.Gather => Gather,
            PlayerActionType.Build => Build,
            PlayerActionType.Weapon => WeaponAction,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

        ActionChanged?.Invoke(state);
    }
}