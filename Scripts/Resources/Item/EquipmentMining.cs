namespace TerrariaRipoffNNF;

public sealed partial class EquipmentMining : EquipmentProperty {
    public float MiningSpeed { get; private set; }
    public float Range { get; private set; }
    public float MiningPower { get; private set; }

    public static Builder New(float miningSpeed, float range, float miningPower) {
        return new Builder(miningSpeed, range, miningPower);
    }

    public class Builder {
        private readonly EquipmentMining _equipmentMining = new();

        public Builder(float miningSpeed, float range, float miningPower) {
            _equipmentMining.Slot = SlotType.Mining;
            _equipmentMining.MiningSpeed = miningSpeed;
            _equipmentMining.Range = range;
            _equipmentMining.MiningPower = miningPower;
        }

        public EquipmentMining Build() {
            return _equipmentMining;
        }
    }
}