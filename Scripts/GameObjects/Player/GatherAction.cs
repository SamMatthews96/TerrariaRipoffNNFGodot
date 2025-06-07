using System;
using Godot;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class GatherAction : PlayerAction {
    [Export] private Player _player;
    public event Action<IntVector, float> GatherAttempted;

    private Game _game;

    private bool _isGathering;
    [Export] private Timer _gatherCooldown;

    public void InitAsLocal(Game game) {
        _game = game;
        _player.ActionController.ActionChanged += OnActionChanged;
        _gatherCooldown.Timeout += OnGatherCooldownTimeout;
    }

    private void OnGatherCooldownTimeout() {
        if (_isGathering) {
            AttemptGather();
        }
    }

    private void OnActionChanged(PlayerActionType _) {
        _isGathering = false;
    }

    public override void PrimaryAction(Vector2 mouseWorldPosition) {
        _isGathering = true;
        if (_gatherCooldown.IsStopped()) {
            AttemptGather();
        }
    }

    private void AttemptGather() {
        IntVector coords = new(_player.GetGlobalMousePosition() / Game.BlockSize);
        if (!_game.IsInBounds(coords)) return;

        if (_player.PlayerEquipment.Pickaxe == null) return;
        if (_player.PlayerEquipment.Pickaxe.Range < IntVector.Distance(coords, Player.Coords)) return;
        RpcId(SceneManager.HostId, nameof(HostGatherAttempted),
            coords.ToSerialised(), _player.PlayerEquipment.Pickaxe.Power);
        _gatherCooldown.Start();
        /* 
            Player is performing an action on a cell
            get the cell contents
            If the cell is a block it needs to be mined
                damage cell based on equipment
                incur gather cooldown based on equipment
            
         */
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void HostGatherAttempted(Array intVectorArray, float damage) {
        IntVector coords = new(intVectorArray);

        GatherAttempted?.Invoke(coords, damage);
    }

    public override void EndPrimaryAction(Vector2 mouseWorldPosition) {
        _isGathering = false;
    }
}