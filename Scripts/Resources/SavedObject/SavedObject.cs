using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class SavedObject : Resource {
    [Export] public Array<ObjectProperty> Properties = new();

    public Dictionary ToDictionary() {
        if (ResourcePath != "") {
            return new Dictionary {
                { "ResourcePath", ResourcePath }
            };
        }

        throw new NotImplementedException();
    }

    public static SavedObject FromDictionary(Dictionary dictionary) {
        if (dictionary.TryGetValue("ResourcePath", out Variant resourcePath)) {
            return ResourceLoader.Load<SavedObject>(resourcePath.AsString());
        }

        throw new NotImplementedException();
    }
}