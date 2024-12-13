using System;
using System.Collections.Generic;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Equipment : ItemProperty {
    private EquipmentSlot _equipmentSlot;
    
    public T GetProperty<T>() where T : EquipmentSlot {
        try {
            return (T) _equipmentSlot;
        } catch (InvalidCastException) {
            throw new KeyNotFoundException($"Equipment does not have property of type {typeof(T)}");
        }
    }
    
    public bool HasProperty<T>() where T : EquipmentSlot {
        return _equipmentSlot is T;
    }

    public override Dictionary ToDictionary() {
        Dictionary serialized = new();
        return serialized;
    }

    public static Builder New(EquipmentSlot equipmentSlot) {
        return new Builder(equipmentSlot);
    }

    public class Builder {
        private readonly Equipment _equipment = new();

        public Builder(EquipmentSlot equipmentSlot) {
            _equipment._equipmentSlot = equipmentSlot;
        }

        public Equipment Build() {
            return _equipment;
        }
    }
}

/*
 *  This class contains all information that all equipment items have in common.
 *  Leave empty for now
 *
 */