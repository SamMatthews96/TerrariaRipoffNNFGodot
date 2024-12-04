using System;
using Godot.Collections;
using Godot;

namespace TerrariaRipoffNNF;

public partial class ActionController : Node {
    [Export] private Player _player;
    private PlayerAction _currentPlayerAction;

    [Export] private GatherAction _gatherAction;
    [Export] private BuildAction _buildAction;

    public event Action<PlayerAction.Type> ActionChanged;
    public event Action<IntVector, float> GatherAttempted;

    public override void _Ready() {
        if (_player.IsLocalPlayer) {
            InputManager.Instance.LeftMouseUp += OnInputManagerLeftMouseUp;
            InputManager.Instance.LeftMouseDown += OnInputManagerLeftMouseDown;
            // InputManager.Instance.RightMouseUp += OnInputManagerRightMouseUp;
            // InputManager.Instance.RightMouseDown += OnInputManagerRightMouseDown;

            Manager.Instance.Game.Interface.ActionBar.ButtonClicked += OnActionBarButtonClicked;

            _gatherAction.GatherAttempted += (coords, damage) =>
                GatherAttempted?.Invoke(coords, damage);

            EquipAction(PlayerAction.Type.Gather);
        }
    }

    private void OnActionBarButtonClicked(PlayerAction.Type state) {
        EquipAction(state);
    }

    private void EquipAction(PlayerAction.Type state) {
        switch (state) {
            case PlayerAction.Type.Gather:
                _currentPlayerAction = _gatherAction;
                break;
            case PlayerAction.Type.Build:
                _currentPlayerAction = _buildAction;
                break;
            case PlayerAction.Type.Weapon:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }

        ActionChanged?.Invoke(state);
    }

    private void OnInputManagerLeftMouseUp(Vector2 mouseScreenPosition) {
        _currentPlayerAction.EndPrimaryAction(mouseScreenPosition);
    }

    private void OnInputManagerLeftMouseDown(Vector2 mouseScreenPosition) {
        _currentPlayerAction.PrimaryAction(mouseScreenPosition);
    }

    private void OnInputManagerRightMouseUp(Vector2 mouseScreenPosition) {
        throw new NotImplementedException();
    }

    private void OnInputManagerRightMouseDown(Vector2 mouseScreenPosition) {
        throw new NotImplementedException();
    }
}