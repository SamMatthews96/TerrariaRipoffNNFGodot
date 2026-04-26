using Godot;

public partial class ReadyOrder : Node {

    public override void _Ready() {
        GD.Print(Name + " is _ready!");
        Ready += () => {
            GD.Print(Name + " ready event!");
        };
    }
}