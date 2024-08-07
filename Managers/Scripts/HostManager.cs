using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class HostManager : Node {
    private SavedBlock[,] _savedBlocks;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public static HostManager Instance { get; private set; }

    public static void Instantiate(Dictionary worldDictionary) {
        if (Manager.Instance.Multiplayer.GetUniqueId() != Manager.MultiplayerHostId) {
            throw new Exception("Cannot instantiate HostManager on client [20240808.0015.1]");
        }

        Instance = Manager.Instance.HostManagerScene.Instantiate<HostManager>();
        Instance.Initialize(worldDictionary);
    }

    private void Initialize(Dictionary worldDictionary) {
        Width = (int)worldDictionary["Width"];
        Height = (int)worldDictionary["Height"];
        _savedBlocks = new SavedBlock[Width, Height];

        Array savedBlockArray = worldDictionary["SavedBlocks"].AsGodotArray();

        foreach (Dictionary savedBlockDictionary in savedBlockArray) {
            SavedBlock savedBlock = SavedBlock.FromDict(savedBlockDictionary);
            _savedBlocks[savedBlock.XPosition, savedBlock.YPosition] = savedBlock;
        }
    }
}