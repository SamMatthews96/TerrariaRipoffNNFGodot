using Godot;

namespace TerrariaRipoffNNF.Managers.Scripts; 

public static class Utils {
    public static Vector2 GetWorldPositionFromCellCoordinates(int x, int y) {
        return new Vector2(x * WorldManager.BLOCK_SIZE, y * WorldManager.BLOCK_SIZE);
    }
}