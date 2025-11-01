using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

public partial class EnemyNpc : CharacterBody2D {
    private float _health = 100;
    [Export] private Area2D _hitbox;

    public static EnemyNpc Create(IntVector spawnCoords) {
        EnemyNpc newEnemyNpc = Data.PackedScenes.TestNpc.Instantiate<EnemyNpc>();
        newEnemyNpc.Position = new Vector2(
            spawnCoords.X * Game.BlockSize,
            spawnCoords.Y * Game.BlockSize
        );

        return newEnemyNpc;
    }

    public override void _Ready() {
        // listen for javelin hit
        // Area Entered and BodyEntered are both events and Area A
        // Area Entered says that an Area B has entered Area A
        // Body Entered says that a body B has entered Area A
        // A body is a Static, Character or Rigid Body.
        
        _hitbox.BodyEntered += OnBodyEntered;
        _hitbox.AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area) {
        GD.Print("ooh my area", area);
    }

    private void OnBodyEntered(Node2D body) {
        GD.Print("ooh my body", body);
    }
}