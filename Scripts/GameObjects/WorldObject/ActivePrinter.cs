using Godot;

namespace TerrariaRipoffNNF;

[GlobalClass]
public partial class ActivePrinter : Node2D {
    public void Action() {
        GD.Print("Bang");
    }
}