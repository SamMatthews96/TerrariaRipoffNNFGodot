namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class SavedBlock {
    private SavedBlock(BlockType blockType, int xPosition, int yPosition) {
        BlockType = blockType;
        XPosition = xPosition;
        YPosition = yPosition;
    }

    public class Builder {
        private SavedBlock _savedBlock;

        public static Builder New(BlockType blockType, int xPosition, int yPosition) {
            SavedBlock savedBlock = new(blockType, xPosition, yPosition);
            return new Builder {
                _savedBlock = savedBlock
            };
        }

        public Builder WithCurrentHealth(float currentHealth) {
            _savedBlock.CurrentHealth = currentHealth;
            return this;
        }

        public SavedBlock Build() {
            if (_savedBlock.CurrentHealth == 0) {
                _savedBlock.CurrentHealth = _savedBlock.BlockType.MaxHealth;
            }
            return _savedBlock;
        }
    }
}