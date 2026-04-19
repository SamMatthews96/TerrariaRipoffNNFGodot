using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlayerManager : Node2D {
    [Export] private World _world;
    
    public event Action<Player> LocalPlayerSpawned;
    
    public void SpawnLocalPlayer(Dictionary playerData) {
        int peerId = Multiplayer.GetUniqueId();
        Player player = Player.Create(peerId, new Vector2I(4, 14));
        player.InitAsLocal(_world.Game, playerData);
        AddChild(player, true);
        LocalPlayerSpawned?.Invoke(player);
        
        // tell other players about the new player
        // server needs to listen for certain events
        // tell new player about other players
    }
}