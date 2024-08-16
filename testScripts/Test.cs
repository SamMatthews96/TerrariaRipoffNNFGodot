using Godot;
using TerrariaRipoffNNF.Scripts.Resources;


namespace TerrariaRipoffNNF.testScripts;

public partial class Test : Node {
    
    public override void _Ready() {
        for (float i = 0; i < 13; i++) {
            GD.Print((int)(i / 10));
        }
    }

    public override void _Process(double delta) { }
}