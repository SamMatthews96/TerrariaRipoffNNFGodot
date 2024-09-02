using Godot;
using TerrariaRipoffNNF.Scripts.Managers.Host;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public partial class Player {
    public class Builder {
        private Player _player;
        private Node _parent;
        private int _peerId = 1;
        private Vector2 _spawnPosition = Vector2.Zero;
        
        public Builder(Node parent, PackedScene packedScene) {
            _player = packedScene.Instantiate<Player>();
            _parent = parent;
        }
        
        public Builder WithPeerId(int peerId) {
            _peerId = peerId;
            return this;
        }
        
        public Builder WithSpawnPosition(Vector2 spawnPosition) {
            _spawnPosition = spawnPosition;
            return this;
        }

        public Player Build() {
            _player.Name = _peerId.ToString();
            _player._spawnPosition = _spawnPosition;
            
            _parent.AddChild(_player, true);
            return _player;
        }
    }

    public static Builder New(Node parent, PackedScene packedScene) {
        HostManager.RequireHost();
        return new Builder(parent, packedScene);
    }
}