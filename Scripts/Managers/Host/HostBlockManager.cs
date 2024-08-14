using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.GameObjects;
using TerrariaRipoffNNF.Scripts.Resources;
using TerrariaRipoffNNF.Scripts.Utils;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Scripts.Managers.Host;

public partial class HostBlockManager : Node {
    public static HostBlockManager Instance { get; private set; }

    [Export] private PackedScene _savedBlockPackedScene;

    public const int BlockSpawnDistance = 20;

    private SavedBlock[,] _savedBlocks;
    private ActiveBlock[,] _activeBlocks;

    [Signal] public delegate void BlockDestroyedEventHandler(SavedBlock savedBlock);

    public override void _EnterTree() {
        if (Instance is not null) {
            throw new Exception("[20240814.0048.1] HostManager already instantiated");
        }

        Instance = this;
    }

    public void Initialize(Dictionary worldDictionary) {
        _savedBlocks = new SavedBlock[
            GameManager.Instance.Width, GameManager.Instance.Height];
        _activeBlocks = new ActiveBlock[
            GameManager.Instance.Width, GameManager.Instance.Height];

        Array savedBlockArray = worldDictionary["SavedBlocks"].AsGodotArray();
        foreach (Dictionary savedBlockDict in savedBlockArray) {
            SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDict);
            _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
        }

        HostPlayerManager.Instance.PlayerSpawned += OnPlayerManagerPlayerSpawned;
    }

    public void SpawnBlocksInRegion(List<IntVector> region) {
        List<SavedBlock> savedBlocks = GetSavedBlocksInRegion(region);
        foreach (SavedBlock savedBlock in savedBlocks) {
            if (_activeBlocks[savedBlock.XPosition, savedBlock.YPosition] is not null) continue;
            SpawnBlock(savedBlock);
        }
    }

    private void SpawnBlock(SavedBlock savedBlock) {
        if (_activeBlocks[savedBlock.XPosition, savedBlock.YPosition] is not null) {
            throw new Exception("[20240814.2208.1] Block already spawned");
        }

        ActiveBlock activeBlock = _savedBlockPackedScene.Instantiate<ActiveBlock>();
        _activeBlocks[savedBlock.XPosition, savedBlock.YPosition] = activeBlock;
        activeBlock.Initialize(savedBlock);
        activeBlock.TakenDamage += OnActiveBlockTakenDamage;
        GameManager.Instance.BlockParent.AddChild(activeBlock, true);
    }

    private List<SavedBlock> GetSavedBlocksInRegion(List<IntVector> region) {
        List<SavedBlock> savedBlocks = new();
        foreach (IntVector coords in region) {
            SavedBlock savedBlock = _savedBlocks[coords.X, coords.Y];
            if (savedBlock is null) continue;
            savedBlocks.Add(savedBlock);
        }

        return savedBlocks;
    }

    private void OnActiveBlockTakenDamage(ActiveBlock activeBlock, float damageAmount) {
        SavedBlock savedBlock = activeBlock.SavedBlock;
        savedBlock.CurrentHealth -= damageAmount;
        if (savedBlock.CurrentHealth > 0) return;
        _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = null;
        activeBlock.QueueFree();

        EmitSignal(SignalName.BlockDestroyed, savedBlock);
    }

    private void OnPlayerManagerPlayerSpawned(Player player) {
        GD.Print("here 123");
        GD.Print(player);
        player.MovedCell += OnLocalPlayerMoved;
    }

    private void OnLocalPlayerMoved(Dictionary positionChange) {
        IntVector oldCoordinates = new(
            (int)positionChange["PreviousX"], (int)positionChange["PreviousY"]);
        IntVector newCoordinates = new(
            (int)positionChange["X"], (int)positionChange["Y"]);
        
        List<IntVector> newRegion = GameManager.Instance.Region.GetRegionDelta(
            newCoordinates, oldCoordinates, BlockSpawnDistance);
        
        SpawnBlocksInRegion(newRegion);

        // List<IntVector> oldRegion = GameManager.Instance.Region.GetRegionDelta(
        //     oldCoordinates, newCoordinates, BlockSpawnDistance);
        // List<SavedBlock> savedBlocksToUnwatch = GetSavedBlocksInRegion(oldRegion);
    }
}