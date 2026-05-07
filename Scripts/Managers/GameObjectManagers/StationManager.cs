using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Scripts.Managers;

public partial class StationManager : Node {
    [Export] private World _world;
    public Dictionary<Vector2I, StationType> Stations = new();
   
    public event Action<Vector2I, StationType> StationCreated;
    public event Action<Vector2I, StationType> StationDestroyed;
    
    public void Register(StationType type, Vector2I coords) {
        Stations.Add(coords, type);
        StationCreated?.Invoke(coords, type);
    }

    public override void _Ready() {
        _world.PropManager.HostPropDestroyed += OnHostPropDestroyed;
        TreeExiting += () => {
            _world.PropManager.HostPropDestroyed -= OnHostPropDestroyed;
        };
    }

    private void OnHostPropDestroyed(ActiveProp _, Vector2I coords) {
        if (!Stations.TryGetValue(coords, out StationType type)) return;
        StationDestroyed?.Invoke(coords, type);
        Stations.Remove(coords);
    }
}