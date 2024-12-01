using Godot;

namespace TerrariaRipoffNNF;

public partial class Player {
    public class Builder {
        private Player _player;
        private int _peerId = 1;
        
        public Builder(PackedScene packedScene) {
            _player = packedScene.Instantiate<Player>();
        }
        
        public Builder WithPeerId(int peerId) {
            _peerId = peerId;
            return this;
        }
        
        public Builder WithSpawnPosition(Vector2 spawnPosition) {
            _player._spawnPosition = spawnPosition;
            return this;
        }

        public Player Build() {
            _player.Name = _peerId.ToString();
            
            Manager.Instance.Game.PlayerParent.AddChild(_player, true);
            return _player;
        }
    }

    public static Builder New(PackedScene packedScene) {
        Host.RequireHost();
        return new Builder(packedScene);
    }
}