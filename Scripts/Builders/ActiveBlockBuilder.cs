using Godot;

namespace TerrariaRipoffNNF;

public partial class ActiveBlock {
    public class Builder {
        private ActiveBlock _activeBlock;
        private Node _parent;
        
        public Builder(Node parent, SavedBlock savedBlock) {
            
            _activeBlock = Manager.Instance.PackedScenes.PackedBlock.Instantiate<ActiveBlock>();
            _activeBlock._savedBlockDictionary = savedBlock.Serialize();
            _parent = parent;
        }
        
        public ActiveBlock Build() {
            _parent.AddChild(_activeBlock, true);
            return _activeBlock;
        }
    }
    
    public static Builder New(Node parent, SavedBlock savedBlock) {
        return new Builder(parent, savedBlock);
    }
}