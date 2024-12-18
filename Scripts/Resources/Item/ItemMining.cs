using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class ItemMining : ItemEquipment {
    [Export] public float Speed { get; private set; }
    [Export] public float Range { get; private set; }
    [Export] public float Power { get; private set; }

    public static ItemMining Create(float speed, float range, float power) {
        return new ItemMining {
            Speed = speed,
            Range = range,
            Power = power
        };
    }
}