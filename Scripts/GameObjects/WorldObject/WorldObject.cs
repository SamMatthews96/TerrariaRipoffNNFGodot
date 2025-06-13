using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class WorldObject : Node2D {
    public IntVector Coords { get; private set; }
    public SavedObject SavedObject { get; private set; }

    public event Action<WorldObject> Destroyed;

    // Blocking vs not blocking
    // CellLayer wall, block or none
    // Leaves pickup on destruction
    // has health
    // has parent
    // has sprite

    public static WorldObject Create(Dictionary data) {
        // @todo
        SavedObject savedObject = new();
        IntVector coords = new();
        return Create(savedObject, coords);
    }

    public static WorldObject Create(
        SavedObject savedObject, IntVector coords) {
        WorldObject worldObject =
            Data.PackedScenes.WorldObject.Instantiate<WorldObject>();
        worldObject.Coords = coords;
        worldObject.SavedObject = savedObject;

        foreach (ObjectProperty objectProperty in
                 worldObject.SavedObject.Properties) {
            objectProperty.OnWorldObjectCreate(worldObject);
        }

        return worldObject;
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

    public bool TryGetProperty<T>(out T property) where T : ObjectProperty {
        foreach (ObjectProperty itemProperty in SavedObject.Properties) {
            if (itemProperty is not T castedProperty) continue;
            property = castedProperty;
            return true;
        }

        property = null;
        return false;
    }

    public T GetProperty<T>() where T : ObjectProperty {
        if (TryGetProperty(out T property)) {
            return property;
        }

        throw new Exception($"Item does not have property of type {typeof(T)}");
    }

    public bool HasProperty<T>() where T : ObjectProperty {
        return TryGetProperty(out T _);
    }
}