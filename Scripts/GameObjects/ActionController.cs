using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TerrariaRipoffNNF.Scripts.Actions;
using TerrariaRipoffNNF.Scripts.Managers;
using TerrariaRipoffNNF.Scripts.UI;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class ActionController : Node {
    private readonly List<(IActionState iActionState, Texture2D texture2D)> _actionIconPairsList = new();
    private IActionState currentActionState;
    [Export] private Player _player;
    [Export] private Texture2D _weaponActionIcon;
    [Export] private Texture2D _gatherActionIcon;
    [Export] private Texture2D _buildActionIcon;


    public override void _Ready() {
        if (!_player.IsLocalPlayer) {
            QueueFree();
            return;
        }

        _actionIconPairsList.Add((new WeaponActionState(_player), _weaponActionIcon));
        _actionIconPairsList.Add((new GatherActionState(_player), _gatherActionIcon));
        _actionIconPairsList.Add((new BuildActionState(_player), _buildActionIcon));

        List<Texture2D> actionIcons =
            _actionIconPairsList.Select(pair => pair.texture2D).ToList();
        UiManager.Instance.ActionBar.Initialize(actionIcons);

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
        GD.Print("equipped " + index);
        currentActionState = _actionIconPairsList[index].iActionState;
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