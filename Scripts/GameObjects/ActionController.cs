using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Scripts.Actions;
using TerrariaRipoffNNF.Scripts.Managers;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class ActionController : Node {
    private List<IAction> _actions = new();
    private IAction _currentAction;
    [Export] private Player _player;
    public List<IAction> Actions => _actions;
    
    [Signal] public delegate void ActionAddedEventHandler();
    

    public override void _Ready() {
        if (!_player.IsLocalPlayer) {
            QueueFree();
            return;
        }
        _actions.Add(new NullAction(_player));
        
        _currentAction = _actions[0];
        _currentAction.Equip();
        
        InputManager.Instance.LeftMouseUp += OnInputManagerLeftMouseUp;
        InputManager.Instance.LeftMouseDown += OnInputManagerLeftMouseDown;
        InputManager.Instance.RightMouseUp += OnInputManagerRightMouseUp;
        InputManager.Instance.RightMouseDown += OnInputManagerRightMouseDown;
        
        
    }
    
    private void AddAction(IAction action) {
        _actions.Add(action);
        EmitSignal(SignalName.ActionAdded);
        
    }

    private void OnInputManagerLeftMouseUp(Vector2 mouseScreenPosition) {
        _currentAction.EndPrimaryAction(mouseScreenPosition);
    }

    private void OnInputManagerLeftMouseDown(Vector2 mouseScreenPosition) {
        _currentAction.PrimaryAction(mouseScreenPosition);
    }

    private void OnInputManagerRightMouseUp(Vector2 mouseScreenPosition) {
        throw new System.NotImplementedException();
    }

    private void OnInputManagerRightMouseDown(Vector2 mouseScreenPosition) {
        throw new System.NotImplementedException();
    }
}