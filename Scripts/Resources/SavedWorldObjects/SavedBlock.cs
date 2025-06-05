// using System;
// using Godot;
// using Godot.Collections;
//
// namespace TerrariaRipoffNNF;
//
// public partial class SavedBlock : SavedWorldObject {
//     public Item Item { get; private init; }
//     public float CurrentHealth { get; set; }
//     
//
//     public override Dictionary ToDictionary() {
//         Dictionary serializedData = new();
//         serializedData.Add("X", XPosition);
//         serializedData.Add("Y", YPosition);
//         serializedData.Add("ResourcePath", Item.ResourcePath);
//         serializedData.Add("CurrentHealth", CurrentHealth);
//         return serializedData;
//     }
//
//     public override ActiveWorldObject SpawnActiveObject() {
//         return ActiveBlock.Create(this);
//     }
//
//     public static SavedBlock FromDictionary(Dictionary dictionary) {
//         return Create(
//             block: Item.FromDictionary(dictionary),
//             xPosition: (int)Math.Round( dictionary["X"].ToString().ToFloat()),
//             yPosition: (int)Math.Round( dictionary["Y"].ToString().ToFloat()),
//             currentHealth: dictionary["CurrentHealth"].ToString().ToFloat()
//         );
//     }
//
//     public static SavedBlock Create(
//         Item block, int xPosition, int yPosition, float currentHealth = 0) {
//         return new SavedBlock {
//             Item = block,
//             XPosition = xPosition,
//             YPosition = yPosition,
//             CurrentHealth = currentHealth == 0
//                 ? block.GetProperty<ItemBlock>().MaxHealth
//                 : currentHealth,
//         };
//     }
// }