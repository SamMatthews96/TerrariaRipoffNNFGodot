using System;
using Godot;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Utils;

public struct IntVector {
    public int X { get; }
    public int Y { get; }

    public IntVector(int x, int y) {
        X = x;
        Y = y;
    }

    public IntVector(Vector2 vector2) {
        X = (int)Math.Round(vector2.X);
        Y = (int)Math.Round(vector2.Y);
    }

    public static IntVector operator -(IntVector a, IntVector b) {
        return new IntVector(a.X - b.X, a.Y - b.Y);
    }

    public Array ToSerialised() {
        return new Array { X, Y };
    }
}