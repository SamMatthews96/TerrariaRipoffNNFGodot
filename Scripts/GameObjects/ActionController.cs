using System;
using Godot.Collections;
using Godot;

namespace TerrariaRipoffNNF;

public partial class ActionController : Node {
    [Export] private Player _player;
    private ActionState _currentActionState;
    
    [Export] private Array<ActionState> _stateArray;
    private Dictionary<PlayerActionState, ActionState> _states;
    
    public event Action<PlayerActionState> ActionChanged;

    public override void _Ready() {
        if (!_player.IsLocalPlayer) {
            QueueFree();
            return;
        }
        
        _states = new Dictionary<PlayerActionState, ActionState>();
        foreach (ActionState state in _stateArray) {
            _states[state.State] = state;
        }

        InputManager.Instance.LeftMouseUp += OnInputManagerLeftMouseUp;
        InputManager.Instance.LeftMouseDown += OnInputManagerLeftMouseDown;
        InputManager.Instance.RightMouseUp += OnInputManagerRightMouseUp;
        InputManager.Instance.RightMouseDown += OnInputManagerRightMouseDown;
        
        Manager.Instance.Game.Interface.ActionBar.ButtonClicked += OnActionBarButtonClicked;

        EquipAction(PlayerActionState.Gather);
    }

    private void OnActionBarButtonClicked(PlayerActionState state) {
        EquipAction(state);
    }

    private void EquipAction(PlayerActionState state) {
        GD.Print("equipped " + state);
        _currentActionState = _states[state];
        ActionChanged?.Invoke(state);
    }

    private void OnInputManagerLeftMouseUp(Vector2 mouseScreenPosition) {
        _currentActionState.EndPrimaryAction(mouseScreenPosition);
    }

    private void OnInputManagerLeftMouseDown(Vector2 mouseScreenPosition) {
        _currentActionState.PrimaryAction(mouseScreenPosition);
    }

    private void OnInputManagerRightMouseUp(Vector2 mouseScreenPosition) {
        throw new NotImplementedException();
    }

    private void OnInputManagerRightMouseDown(Vector2 mouseScreenPosition) {
        throw new NotImplementedException();
    }
}