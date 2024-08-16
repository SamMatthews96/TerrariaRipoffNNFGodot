using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Scripts.Resources;

// using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.testScripts;

public partial class Test : Node {
    
    public override void _Ready() {
        ItemType itemType = ResourceLoader.Load<ItemType>("res://Resources/BlockType/Earth.tres");
        ItemType itemType2 = ResourceLoader.Load<ItemType>("res://Resources/BlockType/Earth.tres");
        
    }

    public override void _Process(double delta) { }
}