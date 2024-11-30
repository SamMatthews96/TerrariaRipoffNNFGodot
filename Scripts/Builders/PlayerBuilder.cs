using Godot;

namespace TerrariaRipoffNNF;

public partial class Player {
    public class Builder {
        private Player _player;
        private Node _parent;
        private int _peerId = 1;
        
        public Builder(Node parent, PackedScene packedScene) {
            _player = packedScene.Instantiate<Player>();
            _parent = parent;
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
            
            _parent.AddChild(_player, true);
            return _player;
        }
    }

    public static Builder New(Node parent, PackedScene packedScene) {
        Host.RequireHost();
        return new Builder(parent, packedScene);
    }
}