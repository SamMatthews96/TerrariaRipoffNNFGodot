using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class WorldCreator : Node {
    // @todo Items belong to Data
    [Export] private Item _stone;
    [Export] private Item _earth;
    [Export] private Item _ferriumOre;
    [Export] private Item _wood;

    public void CreateWorld(WorldBasicInfo worldBasicInfo) {
        Dictionary worldDictionary = new();
        worldDictionary.Add("Name", worldBasicInfo.Name);
        worldDictionary.Add("Width", worldBasicInfo.Width);
        worldDictionary.Add("Height", worldBasicInfo.Height);

        worldDictionary.Add("PlayerPositions", new Array());
        worldDictionary.Add("DefaultSpawnPosition", new Array { 5, 5 });

        int mid = 7;
        Item[] types = { _stone, _earth, _ferriumOre };
        Random random = new();

        // Create world blocks
        Array savedBlockArray = new();
        for (int x = 0; x < worldBasicInfo.Width; x++) {
            for (int y = mid; y < worldBasicInfo.Height; y++) {
                Item type = types[random.Next(3)];
                SavedBlock savedBlock = SavedBlock.Create(
                    block: type,
                    xPosition: x,
                    yPosition: y
                );
                savedBlockArray.Add(savedBlock.ToDictionary());
            }
        }

        worldDictionary.Add("SavedBlocks", savedBlockArray);

        // Create world trees
        Array savedTreeArray = new();
        for (int i = 5; i < 40; i += 10) {
            SavedTree savedTree = SavedTree.Create(_wood, new List<IntVector> {
                new(i, mid + 1),
                new(i, mid + 2),
                new(i, mid + 3),
                new(i, mid + 4),
                new(i, mid + 5),
                new(i, mid + 6),
                new(i, mid + 7),
                new(i, mid + 8)
            });
            savedTreeArray.Add(savedTree.ToDictionary());
        }
        worldDictionary.Add("SavedTrees", savedTreeArray);

        FileManager.SaveWorld(worldDictionary);
    }
}