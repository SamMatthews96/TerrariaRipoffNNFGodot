using System;
using Godot;
using TerrariaRipoffNNF.Scripts.Managers;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Scripts.Utils;

public struct IntVector {
    public bool Equals(IntVector other) {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj) {
        return obj is IntVector other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(X, Y);
    }

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

    public bool IsInBounds() {
        return X >= 0 && X < Manager.Instance.Game.Width && Y >= 0 && Y < Manager.Instance.Game.Height;
    }

    public static IntVector operator -(IntVector a, IntVector b) {
        return new IntVector(a.X - b.X, a.Y - b.Y);
    }

    public static bool operator ==(IntVector a, IntVector b) {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(IntVector a, IntVector b) {
        return !(a == b);
    }

    public Array ToSerialised() {
        return new Array { X, Y };
    }
}