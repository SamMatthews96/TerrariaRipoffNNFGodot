using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class EquipType : ItemType {
    
    public new static BlockType Deserialize(Dictionary dictionary) {
        if (!dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            throw new Exception("[20240815.2158.1] ResourcePath not found in dictionary");
        }
        return ResourceLoader.Load<BlockType>(resourcePath.ToString());
    }
    
    /*
     * Pickaxe:
     * - MiningSpeed
     * - MiningPower
     * Hammer:
     * - BuildSpeed
     *
     * How would special abilities work
     * eg: a pickaxe that can heal on hit
     * An accessory that has an active ability
     *
     * Gather action needs to get information about
     *      pickaxe, axe, etc.
     * Player.Equip.Pickaxe.Power
     *
     * Build action needs
     *      hammer, Player.Equip.Hammer.Speed
     *
     * EquipTypes don't need a resource, at least not yet.
     * They can be created using a Builder pattern?
     *
     * @idea
     * Use only ItemType, and don't use inheritance for any items
     * Use builder pattern to create items with different properties.
      Need to be able to create from a resource, a Dict, and code
     * This needs to work with Resources, as well as items created by code
     * 
        Equipable : ItemProperty
        Equipable pickaxe = Equipable.New()
          .AsMiningTool(float power, float speed)
          .WithPassive();
          

      Item.New()
          .AsEquipable(pickaxe)

      Item earthBlock = Item.New()
          .AsBuildBlock(texture, weight, ...)
          .Build()

      earthBlock.HasProperty(ItemTypes.Block) === true

      class Trait {
        private Equipable _owner;

        public Trait(){

        }
      }




      */
}
[GlobalClass]
public partial class Item : Resource {
  public enum Type {
    Block, Equipment, Weapon, Reagent?
  }
    public List<IItemProperty> ItemProperties { get; private set; }
    public bool HasProperty()
    public static Builder New() {
        return new Builder();
    }
    public class Builder {
        private ItemType _itemType;
        public Builder() {
                
        }

        public Builder AsPickaxe(float speed, float power) {
            // add properties to item
            
            return this;
        }
    }
}

public interface IItemProperty {
    
}
