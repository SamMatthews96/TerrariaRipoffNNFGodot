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
            GD.Print(_player.Name);
            
            InputManager.Instance.LeftMouseUp += OnInputManagerLeftMouseUp;
            InputManager.Instance.LeftMouseDown += OnInputManagerLeftMouseDown;

            Manager.Instance.Game.Interface.ActionBar.ButtonClicked += OnActionBarButtonClicked;

            EquipAction(PlayerAction.Type.Gather);
        }

        if (Manager.Instance.Game.IsHost) {
            _gatherAction.GatherAttempted += (coords, damage) =>
                GatherAttempted?.Invoke(coords, damage);
        }
    }
    
    private void OnInputManagerLeftMouseUp(Vector2 mouseWorldPosition) {
        _currentPlayerAction.EndPrimaryAction(mouseWorldPosition);
    }
    
    private void OnInputManagerLeftMouseDown(Vector2 mouseWorldPosition) {
        _currentPlayerAction.PrimaryAction(mouseWorldPosition);
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
}