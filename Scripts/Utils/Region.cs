using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class Region : Node {
    public List<IntVector> GetRegion(IntVector center, int distanceToEdge) {
        List<IntVector> regionDelta = new();

        int xStart = Math.Max(0, center.X - distanceToEdge);
        int xEnd = Math.Min(SceneManager.Instance.Game.Width - 1, center.X + distanceToEdge);
        int yStart = Math.Max(0, center.Y - distanceToEdge);
        int yEnd = Math.Min(SceneManager.Instance.Game.Height - 1, center.Y + distanceToEdge);

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
        int xEnd = Math.Min(SceneManager.Instance.Game.Width - 1, includeCenter.X + distanceToEdge);
        int yStart = Math.Max(0, includeCenter.Y - distanceToEdge);
        int yEnd = Math.Min(SceneManager.Instance.Game.Height - 1, includeCenter.Y + distanceToEdge);

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