using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class ItemMining : ItemEquipment {
    public float MiningSpeed { get; private set; }
    public float Range { get; private set; }
    public float MiningPower { get; private set; }

    public static Builder New(float miningSpeed, float range, float miningPower) {
        return new Builder(miningSpeed, range, miningPower);
    }

    public class Builder {
        private readonly ItemMining _itemMining = new();

        public Builder(float miningSpeed, float range, float miningPower) {
            _itemMining.Slot = EquipmentSlot.Mining;
            _itemMining.MiningSpeed = miningSpeed;
            _itemMining.Range = range;
            _itemMining.MiningPower = miningPower;
        }

        public ItemMining Build() {
            return _itemMining;
        }
    }
}