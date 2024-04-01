using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Managers.Scripts;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class SavedBlock : Resource {
    [Signal]
    public delegate void HitZeroHealthEventHandler(int xPosition, int yPosition);

    public int XPosition { get; }
    public int YPosition { get; }
    public BlockType BlockType { get; }
    public float CurrentHealth { get; private set; }


    public void TakeDamage(float damageAmount) {
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0) {
            EmitSignal(SignalName.HitZeroHealth, XPosition, YPosition);
        }
    }

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
            BlockType blockType = FileManager.LoadBlockType(dictionary["ResourcePath"].ToString());
            int xPosition = dictionary["X"].ToString().ToInt();
            int yPosition = dictionary["Y"].ToString().ToInt();
            float currentHealth = dictionary["CurrentHealth"].ToString().ToFloat();
            return Builder.New(blockType, xPosition, yPosition)
                .WithCurrentHealth(currentHealth)
                .Build();
        }
        catch (Exception e) {
            GD.Print("error reading SavedBlock from dictionary");
            GD.PrintErr(e);
            throw new NotImplementedException();
        }
    }
}