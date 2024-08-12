using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class HostManager : Node {
    [Export] private HostBlockManager _hostBlockManager;
    
    private Vector2 _defaultSpawnPosition;
    

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
        
        
    }
}