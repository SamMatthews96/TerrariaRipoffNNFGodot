using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public abstract partial class ItemEquipment : ItemProperty {
    public EquipmentSlot Slot { get; protected set; }
}