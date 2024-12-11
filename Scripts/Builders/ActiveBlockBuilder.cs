namespace TerrariaRipoffNNF;

public partial class ActiveBlock {
    public class Builder {
        private readonly ActiveBlock _activeBlock;

        public Builder(SavedBlock savedBlock) {
            _activeBlock = Manager.Instance.PackedScenes.PackedBlock.Instantiate<ActiveBlock>();
            _activeBlock._savedBlockDictionary = savedBlock.Serialize();
        }

        public ActiveBlock Build() {
            Manager.Instance.Game.BlockParent.AddChild(_activeBlock, true);
            return _activeBlock;
        }
    }

    public static Builder New(SavedBlock savedBlock) {
        return new Builder(savedBlock);
    }
}