using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class SavedTree : Resource {
    // a collection of points, each point could be a placeable
    public Item Item { get; private init; }
    public List<IntVector> OccupiedCells { get; private set; }

    public static SavedTree Create(Item item, List<IntVector> occupiedCells) {
        return new SavedTree {
            Item = item,
            OccupiedCells = occupiedCells
        };
    }
    
    public Dictionary ToDictionary() {
        Dictionary serializedData = new();
        serializedData.Add("Item", Item.ToDictionary());
        Array occupiedCellsArray = new();
        foreach (IntVector occupiedCell in OccupiedCells) {
            Array cellCoords = new();
            cellCoords.Add(occupiedCell.X);
            cellCoords.Add(occupiedCell.Y);
            occupiedCellsArray.Add(cellCoords);
        }
        serializedData.Add("OccupiedCells", occupiedCellsArray);
        return serializedData;
    }
    
    public static SavedTree FromDictionary(Dictionary dictionary) {
        Item item = Item.FromDictionary(dictionary["Item"].AsGodotDictionary());
        List<IntVector> occupiedCells = new();
        Array occupiedCellsArray = dictionary["OccupiedCells"].AsGodotArray();
        foreach (Array cellCoords in occupiedCellsArray) {
            int xPosition = (int)Math.Round(cellCoords[0].ToString().ToFloat());
            int yPosition = (int)Math.Round(cellCoords[1].ToString().ToFloat());
            occupiedCells.Add(new IntVector(xPosition, yPosition));
        }
        return new SavedTree {
            Item = item,
            OccupiedCells = occupiedCells
        };
    }
}