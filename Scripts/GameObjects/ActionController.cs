using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Scripts.Actions;
using TerrariaRipoffNNF.Scripts.Managers;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class ActionController : Node {
    private List<IAction> _actions = new();
    private IAction _currentAction = new NullAction();
    [Export] private Player _player;

    public override void _Ready() {
        if (!_player.IsLocalPlayer) {
            QueueFree();
            return;
        }
        
        InputManager.Instance.LeftMouseUp += OnInputManagerLeftMouseUp;
        InputManager.Instance.LeftMouseDown += OnInputManagerLeftMouseDown;
        InputManager.Instance.RightMouseUp += OnInputManagerRightMouseUp;
        InputManager.Instance.RightMouseDown += OnInputManagerRightMouseDown;
    }

    private void OnInputManagerLeftMouseUp(Vector2 mouseScreenPosition) {
        _currentAction.EndPrimaryAction(_player, mouseScreenPosition);
    }

    private void OnInputManagerLeftMouseDown(Vector2 mouseScreenPosition) {
        _currentAction.PrimaryAction(_player, mouseScreenPosition);
    }

    private void OnInputManagerRightMouseUp(Vector2 mouseScreenPosition) {
        throw new System.NotImplementedException();
    }

    private void OnInputManagerRightMouseDown(Vector2 mouseScreenPosition) {
        throw new System.NotImplementedException();
    }
}