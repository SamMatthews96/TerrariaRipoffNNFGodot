using Godot;

namespace TerrariaRipoffNNF;

public partial class ActiveBlock {
    public class Builder {
        private ActiveBlock _activeBlock;
        private Node _parent;
        
        public Builder(Node parent, PackedScene packedScene, SavedBlock savedBlock) {
            _activeBlock = packedScene.Instantiate<ActiveBlock>();
            _activeBlock._savedBlockDictionary = savedBlock.Serialize();
            _parent = parent;
        }
        
        public ActiveBlock Build() {
            _parent.AddChild(_activeBlock, true);
            return _activeBlock;
        }
    }
    
    public static Builder New(Node parent, PackedScene packedScene, SavedBlock savedBlock) {
        return new Builder(parent, packedScene, savedBlock);
    }
}