using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

public partial class EnemyNpc : CharacterBody2D {
    private float _health = 100;
    
    public static EnemyNpc Create(IntVector spawnCoords) {
        EnemyNpc newEnemyNpc = Data.PackedScenes.TestNpc.Instantiate<EnemyNpc>();
        newEnemyNpc.Position = new Vector2(
            spawnCoords.X * Game.BlockSize,
            spawnCoords.Y * Game.BlockSize
        );

        return newEnemyNpc;
    }
}