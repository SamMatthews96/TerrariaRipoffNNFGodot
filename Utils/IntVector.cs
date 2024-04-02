using Godot.Collections;

namespace TerrariaRipoffNNF.Utils;

public struct IntVector {
    public int X { get; }
    public int Y { get; }

    public IntVector(int x, int y) {
        X = x;
        Y = y;
    }

    public static IntVector operator -(IntVector a, IntVector b) {
        return new IntVector(a.X - b.X, a.Y - b.Y);
    }

    public Array ToSerialised() {
        return new Array { X, Y };
    }
}