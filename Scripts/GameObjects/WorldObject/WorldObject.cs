using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class WorldObject : Node2D {
    public IntVector Coords { get; private set; }
    public SavedObject SavedObject { get; private set; }

    public List<ActiveObjectProperty> ActiveProperties { get; private set; }
        = new();

    public event Action<WorldObject> Destroyed;

    public static WorldObject Create(SavedObject savedObject, IntVector coords) {
        WorldObject worldObject =
            Data.PackedScenes.WorldObject.Instantiate<WorldObject>();
        worldObject.Coords = coords;
        worldObject.SavedObject = savedObject;

        return worldObject;
    }

    public override void _Ready() {
        Position = (Coords * Game.BlockSize).ToVector2();
        foreach (ObjectProperty objectProperty in SavedObject.Properties) {
            objectProperty.OnWorldObjectCreate(this);
        }

        foreach (ActiveObjectProperty activeProperty in ActiveProperties) {
            activeProperty.Init();
        }
        
        if (TryGetActiveProperty(out ActiveObjectHealth health)) {
            health.OnHealthHitZero += Destroy;
        } else {
            if (TryGetActiveProperty(out ActiveObjectGatherable gatherable)) {
                gatherable.Gathered += OnGatheredNoHealth;
            }
        }

    }

    private void OnGatheredNoHealth(Player played) {
        Destroy();
    }

    public void Disable() {
        ProcessMode = ProcessModeEnum.Disabled;
        Visible = false;
    }

    public void Enable() {
        ProcessMode = ProcessModeEnum.Inherit;
        Visible = true;
    }

    public void Destroy() {
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

    public bool TryGetActiveProperty<T>(out T property) where T : ActiveObjectProperty {
        foreach (ActiveObjectProperty itemProperty in ActiveProperties) {
            if (itemProperty is not T castedProperty) continue;
            property = castedProperty;
            return true;
        }

        property = null;
        return false;
    }

    public T GetActiveProperty<T>() where T : ActiveObjectProperty {
        if (TryGetActiveProperty(out T property)) {
            return property;
        }

        throw new Exception($"Item does not have active property of type {typeof(T)}");
    }

    public bool HasActiveProperty<T>() where T : ActiveObjectProperty {
        return TryGetActiveProperty(out T _);
    }
}