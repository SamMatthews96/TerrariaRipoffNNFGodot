using System;
using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class WorldObject : Node {
    public IntVector Coords { get; private set; }
    public SavedObject SavedObject { get; private set; }

    public Node2D ParentNode {
        get => _parentNode;
        set {
            if (_parentNode is null) {
                _parentNode = value;
            } else {
                throw new Exception("[20250615.1109.1] ParentNode is already set.");
            }
        }
    }

    private Node2D _parentNode;

    public List<ActiveObjectProperty> ActiveProperties { get; private set; }
        = new();

    public event Action<WorldObject> Destroyed;

    public override void _Ready() {
        ParentNode.Position = (Coords * Game.BlockSize).ToVector2();

        if (TryGetActiveProperty(out ActiveObjectHealth health)) {
            health.OnHealthHitZero += Destroy;
        } else {
            if (TryGetActiveProperty(out ActiveObjectGatherable gatherable)) {
                gatherable.Gathered += OnGatheredNoHealth;
            }
        }
    }

    private void OnGatheredNoHealth(Player player) {
        Destroy();
    }

    public void Disable() {
        ProcessMode = ProcessModeEnum.Disabled;
        ParentNode.Visible = false;
    }

    public void Enable() {
        ProcessMode = ProcessModeEnum.Inherit;
        ParentNode.Visible = true;
    }

    public void Destroy() {
        Destroyed?.Invoke(this);
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