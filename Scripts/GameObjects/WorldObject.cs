using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class WorldObject : Node2D {
    public IntVector Coords;
    protected Array<WorldObjectProperty> Properties = new();

    public event Action<WorldObject> Destroyed;
    
    // Blocking vs not blocking
    // CellLayer wall, block or none
    // Leaves pickup on destruction
    // has health
    // has parent
    // has sprite
    

    public static WorldObject Create(Dictionary data) {
        return data["type"].AsString() switch {
            "block" => Block.Create(data),
            "prop" => Prop.Create(data),
            "placeable" => Placeable.Create(data),
            _ => throw new Exception(
                $"[20250604.2252.1] Unknown ActiveWorldObject type: {data["type"]}")
        };
    }

    public override void _Ready() {
        foreach(WorldObjectProperty property in Properties) {
            
        }
    }

    public void Disable() {
        ProcessMode = ProcessModeEnum.Disabled;
        Visible = false;
    }
    
    public void Enable() {
        ProcessMode = ProcessModeEnum.Inherit;
        Visible = true;
    }

    protected void Destroy() {
        Destroyed?.Invoke(this);
    }

    public bool TryGetProperty<T>(out T property) where T : WorldObjectProperty {
        foreach (WorldObjectProperty itemProperty in Properties) {
            if (itemProperty is not T castedProperty) continue;
            property = castedProperty;
            return true;
        }

        property = null;
        return false;
    }
    
    public T GetProperty<T>() where T : WorldObjectProperty {
        if (TryGetProperty(out T property)) {
            return property;
        }

        throw new Exception($"Item does not have property of type {typeof(T)}");
    }

    public bool HasProperty<T>() where T : WorldObjectProperty {
        return TryGetProperty(out T _);
    }

    
    
}