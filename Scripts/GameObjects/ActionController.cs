using System;
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
    
    public event Action ActionAdded;
    

    public override void _Ready() {
        if (!_player.IsLocalPlayer) {
            QueueFree();
            return;
        }
        _actions.Add(new NullAction(_player));
        
        EquipAction(0);
        
        InputManager.Instance.LeftMouseUp += OnInputManagerLeftMouseUp;
        InputManager.Instance.LeftMouseDown += OnInputManagerLeftMouseDown;
        InputManager.Instance.RightMouseUp += OnInputManagerRightMouseUp;
        InputManager.Instance.RightMouseDown += OnInputManagerRightMouseDown;
        
        
    }
    
    private void AddAction(IAction action) {
        _actions.Add(action);
        ActionAdded?.Invoke();
        /*
         * update the actions in the UI
         * associated with each action should be
         *  - an image
         * since it emits a signal it needs to be a variant type: resource
         *
         * this class tells the UI
         *      the action count
         *      the action image
         *
         * the UI class will set the selected action
         * 
         */
    }
    
    private void EquipAction(int index) {
        _currentAction = _actions[index];
        _currentAction.Equip();
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