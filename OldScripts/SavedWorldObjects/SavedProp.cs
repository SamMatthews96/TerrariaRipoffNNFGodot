// using System;
// using Godot;
// using Godot.Collections;
//
// namespace TerrariaRipoffNNF;
//
// public partial class SavedProp : SavedWorldObject {
//     public Item Item { get; private init; }
//     public float CurrentHealth { get; set; }
//
//     public override Dictionary ToDictionary() {
//         Dictionary serializedData = new();
//         serializedData.Add("Item", Item.ToDictionary());
//         serializedData.Add("XPosition", XPosition);
//         serializedData.Add("YPosition", YPosition);
//         serializedData.Add("CurrentHealth", CurrentHealth);
//         return serializedData;
//     }
//
//     public override ActiveWorldObject SpawnActiveObject() {
//         throw new NotImplementedException();
//     }
//
//     public static SavedProp FromDictionary(Dictionary dictionary) {
//         return Create(
//             item: Item.FromDictionary(dictionary["Item"].AsGodotDictionary()),
//             xPosition: dictionary["XPosition"].ToString().ToInt(),
//             yPosition: dictionary["YPosition"].ToString().ToInt(),
//             currentHealth: dictionary["CurrentHealth"].ToString().ToFloat()
//         );
//     }
//
//     public static SavedProp Create(
//         Item item, int xPosition, int yPosition, float currentHealth = 1) {
//         return new SavedProp {
//             Item = item,
//             XPosition = xPosition,
//             YPosition = yPosition,
//             CurrentHealth = currentHealth
//         };
//     }
// }