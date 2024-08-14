using System;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;
using PlayerInfo = TerrariaRipoffNNF.Scripts.Resources.PlayerInfo;

namespace TerrariaRipoffNNF.Scripts.Managers.Host;

public partial class HostPlayerManager : Node {
    public static HostPlayerManager Instance { get; private set; }
    
    [Export] private PackedScene _hostPlayerPackedScene;
    
    [Signal] public delegate void PlayerSpawnedEventHandler(Player player);

    public override void _EnterTree() {
        if (Instance is not null) {
            throw new Exception("[20240814.0045.1] HostManager already instantiated");
        }

        Instance = this;
    }

    public void SpawnPlayer(int peerId, PlayerInfo playerInfo) {
        Player player = _hostPlayerPackedScene.Instantiate<Player>();
        Vector2 spawnPosition = HostManager.Instance.DefaultSpawnPosition * GameManager.BlockSize;
        player.Initialize(peerId, playerInfo, spawnPosition);

        GameManager.Instance.PlayerParent.AddChild(player, true);
        
        EmitSignal(SignalName.PlayerSpawned, player);
        GD.Print("playerSpawned");
        
    }
}