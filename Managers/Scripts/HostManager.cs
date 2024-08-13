using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Utils;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class HostManager : Node {
    [Export] private HostBlockManager _hostBlockManager;
    
    private Vector2 _defaultSpawnPosition;

    public static void RequireHost() {
        if (!GameManager.Instance.IsHost) {
            throw new Exception("[20240813.1408.1] Method should only be called on the host");
        }
    }
    

    public void Initialize(Dictionary worldDictionary) {
        _defaultSpawnPosition = new Vector2(
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[0].AsDouble(),
            (float)worldDictionary["DefaultSpawnPosition"].AsGodotArray()[1].AsDouble());
        
        _hostBlockManager.Initialize(worldDictionary);
        
        GameManager.Instance.PlayerJoined += OnGameManagerPlayerJoined;
    }

    private void OnGameManagerPlayerJoined(string playerUniqueName) {
        GD.Print("Player joined: " + playerUniqueName);
        GD.Print("spawn position: " + _defaultSpawnPosition);
        
        _hostBlockManager.SpawnLocalBlocks(new IntVector(_defaultSpawnPosition));
        
        
        
    }
}