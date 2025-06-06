// using System.Collections.Generic;
// using Godot;
// using Godot.Collections;
//
// namespace TerrariaRipoffNNF;
//
// public partial class SavedPlaceable : SavedWorldObject {
//     public int XLeftPosition { get; private init; }
//     public int YBottomPosition { get; private init; }
//     public Item Item { get; private init; }
//     public float CurrentHealth { get; set; }
//     public List<IntVector> OccupiedCells { get; private set; }
//
//     public static SavedPlaceable Create(Item item, int xLeftPosition, int yBottomPosition) {
//         List<IntVector> occupiedCells = new();
//         ItemPlaceable itemPlaceable = item.GetProperty<ItemPlaceable>();
//         for (int x = xLeftPosition; x < xLeftPosition + itemPlaceable.Width; x++) {
//             for (int y = yBottomPosition; y < yBottomPosition + itemPlaceable.Height; y++) {
//                 occupiedCells.Add(new IntVector(x, y));
//             }
//         }
//
//         return new SavedPlaceable {
//             Item = item,
//             XLeftPosition = xLeftPosition,
//             YBottomPosition = yBottomPosition,
//             OccupiedCells = occupiedCells
//         };
//     }
//
//     public override Dictionary ToDictionary() {
//         Dictionary serializedData = new();
//         serializedData.Add("Item", Item.ToDictionary());
//         serializedData.Add("XLeftPosition", XLeftPosition);
//         serializedData.Add("YBottomPosition", YBottomPosition);
//         serializedData.Add("CurrentHealth", CurrentHealth);
//         return serializedData;
//     }
//
//     public override ActiveWorldObject SpawnActiveObject() {
//         throw new System.NotImplementedException();
//     }
//
//     public static SavedPlaceable FromDictionary(Dictionary dictionary) {
//         return Create(
//             item: Item.FromDictionary(dictionary["Item"].AsGodotDictionary()),
//             xLeftPosition: dictionary["XLeftPosition"].ToString().ToInt(),
//             yBottomPosition: dictionary["YBottomPosition"].ToString().ToInt()
//         );
//     }
// }