using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.GameObjects.Scripts;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class HostPlayerManager : Node {
    [Export] private PackedScene _hostPlayerPackedScene;
    [Export] private HostManager _hostManager;

    public void Initialize(Dictionary worldDictionary) {
        HostManager.RequireHost();
    }

    public void SpawnPlayer(int peerId, PlayerInfo playerInfo) {
        Player player = _hostPlayerPackedScene.Instantiate<Player>();
        Vector2 spawnPosition = _hostManager.DefaultSpawnPosition * GameManager.BlockSize;
        player.Initialize(peerId, playerInfo, spawnPosition);

        GameManager.Instance.PlayerParent.AddChild(player, true);
    }
}