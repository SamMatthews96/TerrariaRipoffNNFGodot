using System;
using Godot;
using Godot.Collections;
using ActiveBlock = TerrariaRipoffNNF.Scripts.GameObjects.ActiveBlock;

namespace TerrariaRipoffNNF.Scripts.Resources;

public partial class SavedBlock : Resource {

    public int XPosition { get; }
    public int YPosition { get; }
    public BlockType BlockType { get; }
    public float CurrentHealth { get; set; }

    public Dictionary Serialize() {
        Dictionary serializedData = new();
        serializedData.Add("X", XPosition);
        serializedData.Add("Y", YPosition);
        serializedData.Add("ResourcePath", BlockType.ResourcePath);
        serializedData.Add("CurrentHealth", CurrentHealth);
        return serializedData;
    }

    public static SavedBlock FromDict(Dictionary dictionary) {
        try {
            BlockType blockType = ResourceLoader.Load<BlockType>(dictionary["ResourcePath"].ToString());
            int xPosition = dictionary["X"].ToString().ToInt();
            int yPosition = dictionary["Y"].ToString().ToInt();
            float currentHealth = dictionary["CurrentHealth"].ToString().ToFloat();
            return Builder.New(blockType, xPosition, yPosition)
                .WithCurrentHealth(currentHealth)
                .Build();
        } catch (Exception e) {
            GD.PrintErr("error reading SavedBlock from dictionary");
            GD.PrintErr(e);
            throw new NotImplementedException();
        }
    }
}