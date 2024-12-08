// using System;
// using Godot;
// using Godot.Collections;
//
// namespace TerrariaRipoffNNF;
//
// /*
//   All Itemtypes have the basic characteristics required
//     Can be looted, stored, dropped, picked up, etc
//
//   Itemtypes that have special behaviours: Equippable, Reagents, Blocks, etc
//     need to have those behaviours usable, eg
//     A list of Items should be checkable to find items with such characteristic:
//     // for each itemtype in itemtypeList
//     if (!itemType.TryGetBehaviour<Equipable>(out equipable)){
//       return;
//     };
//     equippableItems.add(equippable)
//
//   For now, assume each occurence of a behaviour is unique, 
//     throw an exception when trying to add two Equippables
//
//   Then suppose there is a list of equippable properties 
//   with a reference to the parent item, The player needs to be able to rightclick 
//   the ui item and equip it.
//   Ui item can have a generic emit event on click. Property can have a reference to the
//   parent (item), and the item can listen to emits from Ui item.
//   Problem: if different item types require different actions, eg. equippables being equipped, 
//     and some other type has a different action, then one needs to override the other, making 
//     them not independent events. Therefore, When building an itemType, if the itemType posesses some 
//     characteristic such as equippable, the behaviour should be added to the itemType, not the 
//     equippable property.
//
//   So Itemtype that is equippable is right clicked, UI -> item.OnUiRightClick => () => {
//     // I guess it receives a delegate function from the property or something
//     // the event that equips the item should be fired from the equippable? 
//     Equippable.OnEquip.
//     Maybe a function like Equippable.ListenForRightClick() {
//       _item.OnUiItemClicked += OnItemEquipAttempt;
//     }
//     that is public, that causes equipable to listen to the right event
//     where OnItemEquipAttempt is private, 
//     // a function that creates the listener is at least easy to track if it is 
//     // invoked in the wrong place, and potentially could be made internal if 
//     // itemTypes belonged to their own assembly.
//
//
//
//     public ItemType Build(){
//       // if equippable property
//       equippable.ListenForRightClick;
//       return;
//       // else if ??? property
//       // do something else
//
//       
//     }
//
//     So, equippable items now run a private OnEquip method ...
//     This should emit an event. The player could be listening for such events.
//     Though, every time an inventory item is added, would be player be required to do a check for 
//     TryGetProperty?. (which is no less clunky than the inheritance model used before)
//
//     Player _ready:
//       // inventoryChanged should probably be split into a few events, 
//         added itemStack,
//         addedToItemStack
//         removed stack, removedFromStack
//       Inventory.AddedStack += (itemStack) => {
//         itemStack.itemType.TryGetProperty<Equippable>().Equipped += OnItemEquipped;
//       }
//
//       OnItemEquipped(Equippable equippable){
//         // different equippables with have different effects, 
//         // and those effects need to trigger inside player(or some property thereof)
//         
//       }
//
//
//
//   }
//
//   Equippable items need the following traits:
//     slot, unique (can be enum? head, chest, 1h, 2h, miningTool, etc)
//     On equip (items may have stats)
//     On unequip
//
//
//
//   
// */
// public partial class ItemType : Resource {
//     [Export] public float InventorySpace { get; private set; }
//     [Export] public bool IsStackable { get; private set; } = true;
//     [Export] public Texture2D IconTexture { get; private set; }
//     [Export] public float FallWeight { get; private set; }
//     [Export] public List<ItemTypeProperty> Properties { get; private set; }
//
//     public class Builder {
//       private ItemType _itemType;
//       private float _inventorySpace;
//       private bool _isStackable;
//       private List<ItemTypeProperty> _properties;
//
//       public Builder(){
//         _itemType = new ItemType()
//       }
//
//       // 
//
//       public Builder WithProperty(ItemTypeProperty newProperty){
//         _properties.Add(newProperty)
//       }
//
//       public ItemType Build(){
//
//       }
//
//     }
//     public static Builder New(float inventorySpace, Texture2D iconTexture){
//       return new Builder();
//     }
//
//     public virtual Dictionary Serialize() {
//         Dictionary serialized = new();
//         serialized.Add("ResourcePath", ResourcePath);
//         return serialized;
//     }
//
//     public static ItemType Deserialize(Dictionary dictionary) {
//         if (!dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
//             throw new Exception("[20240815.2158.1] ResourcePath not found in dictionary");
//         }
//         
//         return ResourceLoader.Load<ItemType>(resourcePath.ToString());
//     }
// }
//
// public interface IItemTypeProperty{
//
// }
//
// public class Equipable : IItemTypeProperty {
//   public EquipableProperty EquipableProperty {get; private set;}
//   public List<OnEquip> onEquipList;
//   public static Builder New(){
//     return new Builder();
//   }
//   public class Builder(){
//
//
//     public Equipable Build(){
//
//     }
//   }
// }