using System;
using Godot;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class IntVector : Resource {
    public bool Equals(IntVector other) {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj) {
        return obj is IntVector other && Equals(other);
    }

    public override int GetHashCode() {
        return HashCode.Combine(X, Y);
    }

    [Export] public int X { get; private set; }
    [Export] public int Y { get; private set; }

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

    public IntVector() { }

    public static IntVector operator -(IntVector a, IntVector b) {
        return new IntVector(a.X - b.X, a.Y - b.Y);
    }

    public static bool operator ==(IntVector a, IntVector b) {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(IntVector a, IntVector b) {
        return !(a == b);
    }
    
    public static IntVector operator +(IntVector a, IntVector b) {
        return new IntVector(a.X + b.X, a.Y + b.Y);
    }
    
    public static IntVector operator *(IntVector a, int b) {
        return new IntVector(a.X * b, a.Y * b);
    }
    
    public Vector2 ToVector2() {
        return new Vector2(X, Y);
    } 
    
    public static float Distance(IntVector a, IntVector b) {
        return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
    }
}
