namespace TerrariaRipoffNNF;

public sealed partial class MiningSlot : EquipmentSlot {
    public float MiningSpeed { get; private set; }
    public float Range { get; private set; }
    public float MiningPower { get; private set; }

    public static Builder New(float miningSpeed, float range, float miningPower) {
        return new Builder(miningSpeed, range, miningPower);
    }

    public class Builder {
        private readonly MiningSlot _miningSlot = new();

        public Builder(float miningSpeed, float range, float miningPower) {
            _miningSlot.Slot = SlotType.Mining;
            _miningSlot.MiningSpeed = miningSpeed;
            _miningSlot.Range = range;
            _miningSlot.MiningPower = miningPower;
        }

        public MiningSlot Build() {
            return _miningSlot;
        }
    }
}