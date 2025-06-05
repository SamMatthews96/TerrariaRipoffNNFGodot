using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public abstract partial class ActiveWorldObject : Node2D {
    public int XPosition { get; protected set; }
    public int YPosition { get; protected set; }

    public static ActiveWorldObject Create(Dictionary data) {
        ActiveWorldObject newObject;
        switch (data["type"].AsString()) {
            case "block":
                newObject = ActiveBlock.Create(data);
                break;
            // case "prop":
            //     newObject = ActiveProp.Create(data);
            //     break;
            default:
                throw new Exception($"[20250604.2252.1] Unknown ActiveWorldObject type: {data["type"]}");
        }
        newObject.XPosition = (int)Math.Round(data["xPosition"].ToString().ToFloat());
        newObject.YPosition = (int)Math.Round(data["yPosition"].ToString().ToFloat());
        return newObject;
    }
    
    
}