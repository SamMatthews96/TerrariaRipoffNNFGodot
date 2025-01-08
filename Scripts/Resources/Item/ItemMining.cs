using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public sealed partial class ItemMining : ItemEquipment {
    public override PropertyType PropertyType => PropertyType.Mining;
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
    
    public new static ItemMining FromDictionary(Dictionary dictionary) {
        return Create(
            float.Parse(dictionary["Speed"].ToString()),
            float.Parse(dictionary["Range"].ToString()),
            float.Parse(dictionary["Power"].ToString())
        );
    }

    public override Dictionary ToDictionary() {
        Dictionary serialized = new();
        serialized.Add("PropertyType", PropertyType.ToString());
        serialized.Add("Speed", Speed);
        serialized.Add("Range", Range);
        serialized.Add("Power", Power);
        return serialized;
    }

    public override Dictionary GetTooltipAttributes() {
        Dictionary tooltipAttributes = new();
        tooltipAttributes.Add("Speed", Speed);
        tooltipAttributes.Add("Range", Range);
        tooltipAttributes.Add("Power", Power);
        return tooltipAttributes;
    }
}