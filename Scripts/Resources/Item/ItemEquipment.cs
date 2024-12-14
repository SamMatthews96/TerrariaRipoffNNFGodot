using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public enum EquipmentSlot {
    Mining,
    Weapon,
    Armor,
    Accessory
}

[GlobalClass]
public abstract partial class ItemEquipment : ItemProperty {
    public EquipmentSlot Slot { get; protected set; }

    public override Dictionary ToDictionary() {
        throw new NotImplementedException();
    }
}