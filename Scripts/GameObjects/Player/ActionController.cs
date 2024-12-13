using System;
using Godot;

namespace TerrariaRipoffNNF;

public partial class ActionController : Node {
    [Export] private Player _player;
    private PlayerAction _currentPlayerAction;

    [Export] private GatherAction _gatherAction;
    [Export] private BuildAction _buildAction;

    public event Action<PlayerAction.Type> ActionChanged;
    public event Action<IntVector, float> GatherAttempted;
    public event Action<Item, IntVector> BlockPlaced;

    public override void _Ready() {
        if (_player.IsLocalPlayer) {
            Manager.Instance.Game.InputManager.LeftMouseUp += OnInputManagerLeftMouseUp;
            Manager.Instance.Game.InputManager.LeftMouseDown += OnInputManagerLeftMouseDown;
            Manager.Instance.Game.Interface.ActionBar.ButtonClicked += EquipAction;
            Manager.Instance.Game.InputManager.PlayerActionModeChanged += EquipAction;

            EquipAction(PlayerAction.Type.Gather);
        }

        if (Manager.Instance.Game.IsHost) {
            _gatherAction.GatherAttempted += OnGatherAttempted;
            _buildAction.BlockPlaced += OnBlockPlaced;
        }
    }

    public override void _ExitTree() {
        if (_player.IsLocalPlayer) {
            Manager.Instance.Game.InputManager.LeftMouseUp -= OnInputManagerLeftMouseUp;
            Manager.Instance.Game.InputManager.LeftMouseDown -= OnInputManagerLeftMouseDown;
            Manager.Instance.Game.Interface.ActionBar.ButtonClicked -= EquipAction;
            Manager.Instance.Game.InputManager.PlayerActionModeChanged -= EquipAction;
        }

        if (Manager.Instance.Game.IsHost) {
            _gatherAction.GatherAttempted -= OnGatherAttempted;
            _buildAction.BlockPlaced -= OnBlockPlaced;
        }
    }

    private void OnInputManagerLeftMouseUp(Vector2 mouseWorldPosition) {
        _currentPlayerAction.EndPrimaryAction(mouseWorldPosition);
    }

    private void OnInputManagerLeftMouseDown(Vector2 mouseWorldPosition) {
        _currentPlayerAction.PrimaryAction(mouseWorldPosition);
    }

    private void EquipAction(PlayerAction.Type state) {
        _currentPlayerAction = state switch {
            PlayerAction.Type.Gather => _gatherAction,
            PlayerAction.Type.Build => _buildAction,
            PlayerAction.Type.Weapon => throw new NotImplementedException(),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

        ActionChanged?.Invoke(state);
    }

    private void OnGatherAttempted(IntVector coords, float damage) {
        GatherAttempted?.Invoke(coords, damage);
    }

    private void OnBlockPlaced(Item blockType, IntVector coords) {
        BlockPlaced?.Invoke(blockType, coords);
    }
}