using System;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class WorldCreator : Node {
    [Export] private Item _stone;
    [Export] private Item _earth;

    public void CreateWorld(WorldBasicInfo worldBasicInfo) {
        int mid = 7;

        Item[] types = { _stone, _earth };
        Random random = new();

        Array savedBlockArray = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                Item type = types[random.Next(2)];
                SavedBlock savedBlock = SavedBlock.Builder.New(type, x, y).Build();
                savedBlockArray.Add(savedBlock.Serialize());
            }
        }

        int defaultSpawnX = 5;
        int defaultSpawnY = 5;

        Dictionary worldDictionary = new();
        worldDictionary.Add("Name", worldBasicInfo.Name);
        worldDictionary.Add("Width", worldBasicInfo.Width);
        worldDictionary.Add("Height", worldBasicInfo.Height);
        worldDictionary.Add("SavedBlocks", savedBlockArray);
        worldDictionary.Add("PlayerPositions", new Array());
        worldDictionary.Add("DefaultSpawnPosition", new Array { defaultSpawnX, defaultSpawnY });
        FileManager.SaveWorld(worldDictionary);
    }
}