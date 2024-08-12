using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.GameObjects.Scripts;
using TerrariaRipoffNNF.Managers.Scripts;
using TerrariaRipoffNNF.Utils;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class SavedBlock : Resource, ISavedGameObject {
    [Signal]
    public delegate void HitZeroHealthEventHandler(SavedBlock savedBlock);

    [Signal]
    public delegate void ActiveBlockCreatedEventHandler(ActiveBlock activeBlock);

    public int XPosition { get; }
    public int YPosition { get; }
    public BlockType BlockType { get; }
    public float CurrentHealth { get; private set; }
    public ActiveBlock ActiveBlock { get; private set; }
    public IntVector GridPosition { get; }

    public void TakeDamage(float damageAmount) {
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0) {
            EmitSignal(SignalName.HitZeroHealth, this);
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
            GD.PrintErr("error reading SavedBlock from dictionary");
            GD.PrintErr(e);
            throw new NotImplementedException();
        }
    }
}