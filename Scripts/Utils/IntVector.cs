using System;
using Godot;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public readonly struct IntVector {
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

    public IntVector(Array array) {
        try {
            X = (int)array[0];
            Y = (int)array[1];
        } catch (Exception e) {
            throw new Exception("[20241204.0957.1] Error deserializing IntVector from Array", e);
        }
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
    
    public static float Distance(IntVector a, IntVector b) {
        return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
    }

    public Array ToSerialised() {
        return new Array { X, Y };
    }
}