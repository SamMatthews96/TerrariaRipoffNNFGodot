using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class ItemMining : ItemEquipment {
    [Export] public float Speed { get; private set; }
    [Export] public float Range { get; private set; }
    [Export] public float Power { get; private set; }

    public static Builder New() {
        return new Builder();
    }

    public class Builder {
        private readonly ItemMining _itemMining = new();

        public Builder() {
            _itemMining.Slot = EquipmentSlot.Mining;
        }

        public Builder SetMiningSpeed(float miningSpeed) {
            _itemMining.Speed = miningSpeed;
            return this;
        }

        public Builder SetRange(float range) {
            _itemMining.Range = range;
            return this;
        }

        public Builder SetMiningPower(float miningPower) {
            _itemMining.Power = miningPower;
            return this;
        }

        public ItemMining Build() {
            return _itemMining;
        }
    }
}