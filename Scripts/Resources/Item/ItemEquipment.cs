using System;
using System.Collections.Generic;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public sealed partial class ItemEquipment : ItemProperty {
    private EquipmentProperty _equipmentProperty;
    
    public T GetProperty<T>() where T : EquipmentProperty {
        try {
            return (T) _equipmentProperty;
        } catch (InvalidCastException) {
            throw new KeyNotFoundException($"Equipment does not have property of type {typeof(T)}");
        }
    }
    
    public bool HasProperty<T>() where T : EquipmentProperty {
        return _equipmentProperty is T;
    }

    public override Dictionary ToDictionary() {
        Dictionary serialized = new();
        return serialized;
    }

    public static Builder New(EquipmentProperty equipmentProperty) {
        return new Builder(equipmentProperty);
    }

    public class Builder {
        private readonly ItemEquipment _itemEquipment = new();

        public Builder(EquipmentProperty equipmentProperty) {
            _itemEquipment._equipmentProperty = equipmentProperty;
        }

        public ItemEquipment Build() {
            return _itemEquipment;
        }
    }
}

/*
 *  This class contains all information that all equipment items have in common.
 *  Leave empty for now
 *
 */