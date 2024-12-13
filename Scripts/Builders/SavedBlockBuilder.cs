namespace TerrariaRipoffNNF;

public partial class SavedBlock {
    private SavedBlock() { }

    public class Builder {
        private SavedBlock _savedBlock;

        public static Builder New(Item block, int xPosition, int yPosition) {
            SavedBlock savedBlock = new() {
                Item = block,
                XPosition = xPosition,
                YPosition = yPosition
            };
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
                _savedBlock.CurrentHealth = _savedBlock.Item.GetProperty<ItemBlock>().MaxHealth;
            }

            return _savedBlock;
        }
    }
}