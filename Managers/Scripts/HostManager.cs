using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;
using TerrariaRipoffNNF.Utils;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class HostManager : Node {
    [Export] private HostBlockManager _hostBlockManager;
    [Export] private HostPlayerManager _hostPlayerManager;
    
    public Vector2 DefaultSpawnPosition { get; private set; }

    public static void RequireHost() {
        if (!GameManager.Instance.IsHost) {
            throw new Exception("[20240813.1408.1] Method should only be called on the host");
        }
    }

    public void Initialize(Dictionary worldDictionary) {
        RequireHost();
        
        DefaultSpawnPosition = new Vector2(
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[0].AsDouble(),
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[1].AsDouble());
        
        _hostBlockManager.Initialize(worldDictionary);
        _hostPlayerManager.Initialize(worldDictionary);
        
        GameManager.Instance.PlayerJoined += OnGameManagerPlayerJoined;
    }

    private void OnGameManagerPlayerJoined(PlayerInfo playerInfo, int peerId) {
        
        _hostBlockManager.SpawnLocalBlocks(new IntVector(DefaultSpawnPosition));
        _hostPlayerManager.SpawnPlayer(peerId, playerInfo);
        
    }
}