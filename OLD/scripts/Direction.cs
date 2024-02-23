using System;

namespace TerrariaRipoffNNF.scripts;

public enum Direction {
    Up,
    Down,
    Left,
    Right
}

public static class DirectionMethods {
    public static Direction Opposite(Direction direction) {
        return direction switch {
            Direction.Down => Direction.Up,
            Direction.Up => Direction.Down,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}