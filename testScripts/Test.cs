using Godot;

namespace TerrariaRipoffNNF;

public partial class Test : Node {
    public override void _Ready() {
        Item myRes = new Item();
        Item anotherRes = new Item();
        GD.Print(myRes == anotherRes);
        
        Block myRes2 = ResourceLoader.Load<Block>("res://Resources/ItemProperties/Block/earthBlock.tres");
        Block myRes3 = ResourceLoader.Load<Block>("res://Resources/ItemProperties/Block/earthBlock.tres");
        // myRes2.Weight = 200f;
        GD.Print(myRes2 == myRes3);
    } 
}