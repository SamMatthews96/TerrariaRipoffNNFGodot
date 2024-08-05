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
    private readonly List<Node> _nodesWatchingThisBlock = new();

    [Signal]
    public delegate void HitZeroHealthEventHandler(int xPosition, int yPosition);

    public int XPosition { get; }
    public int YPosition { get; }
    public BlockType BlockType { get; }
    public float CurrentHealth { get; private set; }
    public ActiveBlock ActiveBlock { get; private set; }
    public IntVector GridPosition { get; }

    public bool ShouldCreateActiveBlock => _nodesWatchingThisBlock.Count >= 0 && ActiveBlock is null;
    public bool ShouldDeleteActiveBlock => ActiveBlock is not null && _nodesWatchingThisBlock.Count == 0;

    public void AddActiveBlock(ActiveBlock activeBlock) {
        ActiveBlock = activeBlock;
    }

    public void RemoveActiveBlock() {
        ActiveBlock = null;
    }

    public void TakeDamage(float damageAmount) {
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0) {
            EmitSignal(SignalName.HitZeroHealth, XPosition, YPosition);
        }
    }

    public void AddWatcher(Node watcher) {
        if (_nodesWatchingThisBlock.Contains(watcher)) return;
        _nodesWatchingThisBlock.Add(watcher);
    }

    public void RemoveWatcher(Node watcher) {
        if (!_nodesWatchingThisBlock.Contains(watcher)) return;
        _nodesWatchingThisBlock.Remove(watcher);
    }

    public Dictionary Serialize() {
        Dictionary serializedData = new();
        serializedData.Add("X", XPosition);
        serializedData.Add("Y", YPosition);
        serializedData.Add("ResourcePath", BlockType.ResourcePath);
        serializedData.Add("CurrentHealth", CurrentHealth);
        return serializedData;
    }

    public static Array SerializeArray(SavedBlock[,] savedBlocks) {
        Array serializedArray = new();
        foreach (SavedBlock savedBlock in savedBlocks) {
            if (savedBlock is null) continue;
            serializedArray.Add(savedBlock.Serialize());
        }

        return serializedArray;
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