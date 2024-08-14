using System;
using System.Collections.Generic;
using Godot;
using GameManager = TerrariaRipoffNNF.Scripts.Managers.GameManager;

namespace TerrariaRipoffNNF.Scripts.Utils;

public partial class Region : Node {
    [Export] private GameManager _gameManager;
    
    public List<IntVector> GetRegion(IntVector center, int distanceToEdge) {
        List<IntVector> regionDelta = new();

        int xStart = Math.Max(0, center.X - distanceToEdge);
        int xEnd = Math.Min(_gameManager.Width - 1, center.X + distanceToEdge);
        int yStart = Math.Max(0, center.Y - distanceToEdge);
        int yEnd = Math.Min(_gameManager.Height - 1, center.Y + distanceToEdge);

        for (int x = xStart; x < xEnd; x++) {
            for (int y = yStart; y < yEnd; y++) {
                regionDelta.Add(new IntVector(x, y));
            }
        }

        return regionDelta;
    }

    public List<IntVector> GetRegionDelta(IntVector includeCenter, IntVector excludeCenter, int distanceToEdge) {
        List<IntVector> regionDelta = new();

        int xStart = Math.Max(0, includeCenter.X - distanceToEdge);
        int xEnd = Math.Min(_gameManager.Width - 1, includeCenter.X + distanceToEdge);
        int yStart = Math.Max(0, includeCenter.Y - distanceToEdge);
        int yEnd = Math.Min(_gameManager.Height - 1, includeCenter.Y + distanceToEdge);

        for (int x = xStart; x < xEnd; x++) {
            for (int y = yStart; y < yEnd; y++) {
                if (Math.Abs(x - excludeCenter.X) < distanceToEdge &&
                    Math.Abs(y - excludeCenter.Y) < distanceToEdge) continue;
                regionDelta.Add(new IntVector(x, y));
            }
        }

        return regionDelta;
    }
}