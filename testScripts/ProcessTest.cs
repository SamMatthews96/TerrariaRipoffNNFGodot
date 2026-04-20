using Godot;
using System;

// is there a flag that disables process
// yes
// ProcessMode = ProcessModeEnum.Always;

public partial class ProcessTest : Node {
    
    public override void _Process(double delta) {
        GD.Print("Process");
    }
}