// using System;
// using Godot;
// using Godot.Collections;
//
// namespace TerrariaRipoffNNF;
//
// [GlobalClass]
// public partial class SavedObject : Resource {
//     [Export] public Array<ObjectProperty> Properties = new();
//
//     public Dictionary ToDictionary() {
//         if (ResourcePath != "") {
//             return new Dictionary {
//                 { "ResourcePath", ResourcePath }
//             };
//         }
//
//         throw new NotImplementedException();
//     }
//
//     public static SavedObject FromDictionary(Dictionary dictionary) {
//         if (dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
//             return ResourceLoader.Load<SavedObject>(resourcePath.AsString());
//         }
//
//         throw new NotImplementedException();
//     }
//     
//     public bool TryGetProperty<T>(out T property) where T : ObjectProperty {
//         foreach (ObjectProperty itemProperty in Properties) {
//             if (itemProperty is not T castedProperty) continue;
//             property = castedProperty;
//             return true;
//         }
//
//         property = null;
//         return false;
//     }
//     
//     public bool HasProperty<T>() where T : ObjectProperty {
//         return TryGetProperty(out T _);
//     }
// }