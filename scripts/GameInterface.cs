using Godot;

namespace TerrariaRipoffNNF.scripts; 

public partial class GameInterface : Control {
    [Export] public ProgressBar HealthBar;

    private Player player;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        HealthBar.MaxValue = player.Health.MaxHealth;
        HealthBar.Value = player.Health.CurrentHealth;
    }

    public void SetPlayer(Player newPlayer) {
        player = newPlayer;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}