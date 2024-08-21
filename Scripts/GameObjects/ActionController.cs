using System;
using Godot.Collections;
using Godot;
using TerrariaRipoffNNF.Scripts.Managers;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class ActionController : Node {
    [Export] private Array<ActionState> _actions;
    [Export] private Player _player;
    private ActionState currentActionState;

    public override void _Ready() {
        if (!_player.IsLocalPlayer) {
            QueueFree();
            return;
        }

        InputManager.Instance.LeftMouseUp += OnInputManagerLeftMouseUp;
        InputManager.Instance.LeftMouseDown += OnInputManagerLeftMouseDown;
        InputManager.Instance.RightMouseUp += OnInputManagerRightMouseUp;
        InputManager.Instance.RightMouseDown += OnInputManagerRightMouseDown;

        UiManager.Instance.ActionBar.ButtonClicked += OnActionBarButtonClicked;

        EquipAction(0);
    }

    private void OnActionBarButtonClicked(int index) {
        EquipAction(index);
    }

    private void EquipAction(int index) {
        currentActionState?.Unequip();
        currentActionState = _actions[index];
        currentActionState.Equip();
    }

    private void OnInputManagerLeftMouseUp(Vector2 mouseScreenPosition) {
        currentActionState.EndPrimaryAction(mouseScreenPosition);
    }

    private void OnInputManagerLeftMouseDown(Vector2 mouseScreenPosition) {
        currentActionState.PrimaryAction(mouseScreenPosition);
    }

    private void OnInputManagerRightMouseUp(Vector2 mouseScreenPosition) {
        throw new NotImplementedException();
    }

    private void OnInputManagerRightMouseDown(Vector2 mouseScreenPosition) {
        throw new NotImplementedException();
    }
}