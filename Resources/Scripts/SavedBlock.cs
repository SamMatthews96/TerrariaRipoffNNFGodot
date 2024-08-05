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
    private bool _isActive;

    [Signal]
    public delegate void HitZeroHealthEventHandler(int xPosition, int yPosition);

    [Signal]
    public delegate void WatchersBecomeNonZeroEventHandler(SavedBlock savedBlock);

    [Signal]
    public delegate void WatchersBecomeZeroEventHandler(SavedBlock savedBlock);

    public int XPosition { get; }
    public int YPosition { get; }
    public BlockType BlockType { get; }
    public float CurrentHealth { get; private set; }

    public IntVector GridPosition { get; }


    public void TakeDamage(float damageAmount) {
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0) {
            EmitSignal(SignalName.HitZeroHealth, XPosition, YPosition);
        }
    }

    public void AddWatcher(Node watcher) {
        if (_nodesWatchingThisBlock.Contains(watcher)) return;
        _nodesWatchingThisBlock.Add(watcher);
        if (_isActive) return;
        _isActive = true;
        EmitSignal(SignalName.WatchersBecomeNonZero, this);
    }

    public void RemoveWatcher(Node watcher) {
        GD.Print("removing watcher");
        if (!_nodesWatchingThisBlock.Contains(watcher)) return;
        _nodesWatchingThisBlock.Remove(watcher);
        if (_nodesWatchingThisBlock.Count != 0 || !_isActive) return;
        EmitSignal(SignalName.WatchersBecomeZero, this);
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