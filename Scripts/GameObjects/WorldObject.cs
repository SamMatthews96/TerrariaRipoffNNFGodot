using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public abstract partial class WorldObject : Node2D {
    public int XPosition { get; protected set; }
    public int YPosition { get; protected set; }

    public static WorldObject Create(Dictionary data) {
        return data["type"].AsString() switch {
            "block" => Block.Create(data),
            _ => throw new Exception(
                $"[20250604.2252.1] Unknown ActiveWorldObject type: {data["type"]}")
        };
    }

    public void Disable() {
        ProcessMode = ProcessModeEnum.Disabled;
        Visible = false;
    }
    
    public void Enable() {
        ProcessMode = ProcessModeEnum.Inherit;
        Visible = true;
    }
    
    
}