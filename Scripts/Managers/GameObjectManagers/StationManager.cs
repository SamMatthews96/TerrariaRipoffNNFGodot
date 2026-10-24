using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Scripts.Managers;

public partial class StationManager : Node {
    [Export] private World _world;
    private PropManager _propManager;

    private Dictionary<Vector2I, StationType> _stations = new();

    public event Action<Vector2I, StationType> StationCreated;
    public event Action<Vector2I, StationType> StationDestroyed;

    public void Register(StationType type, Vector2I coords) {
        _stations.Add(coords, type);
        StationCreated?.Invoke(coords, type);
    }

    public override void _Ready() {
        _propManager = _world.PropManager;

        _propManager.HostPropDestroyed += OnHostPropDestroyed;
        TreeExiting += () => { _propManager.HostPropDestroyed -= OnHostPropDestroyed; };
    }

    public Dictionary<Vector2I, StationType> GetStationsInRegion(
        Array<Vector2I> region) {
        Dictionary<Vector2I, StationType> localStations = new();
        foreach (Vector2I coords in region) {
            if (_stations.TryGetValue(coords, out StationType type)) {
                localStations[coords] = type;
            }
        }

        return localStations;
    }

    private void OnHostPropDestroyed(ActiveProp _, Vector2I coords) {
        if (!_stations.TryGetValue(coords, out StationType type)) return;
        StationDestroyed?.Invoke(coords, type);
        _stations.Remove(coords);
    }
}