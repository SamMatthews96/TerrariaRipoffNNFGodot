using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class SavedBlock : Resource {

    public int XPosition { get; private init; }
    public int YPosition { get; private init; }
    public Item Item { get; private init; }
    public float CurrentHealth { get; set; }

    public Dictionary Serialize() {
        Dictionary serializedData = new();
        serializedData.Add("X", XPosition);
        serializedData.Add("Y", YPosition);
        serializedData.Add("ResourcePath", Item.ResourcePath);
        serializedData.Add("CurrentHealth", CurrentHealth);
        return serializedData;
    }

    public static SavedBlock FromDict(Dictionary dictionary) {
        try {
            Item block = Item.FromDictionary(dictionary);
            int xPosition = dictionary["X"].ToString().ToInt();
            int yPosition = dictionary["Y"].ToString().ToInt();
            float currentHealth = dictionary["CurrentHealth"].ToString().ToFloat();
            return Builder.New(block, xPosition, yPosition)
                .WithCurrentHealth(currentHealth)
                .Build();
        } catch (Exception e) {
            GD.PrintErr("error reading SavedBlock from dictionary");
            GD.PrintErr(e);
            throw new NotImplementedException();
        }
    }
}