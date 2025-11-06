using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class WorldObjectLoader : Node {
    private const float LoadTimeoutMs = 16f;
    
    private int _currentLoadCellCount;
    private bool _isStartAreaLoading;
    
    private List<(int x, int y)> _loadingQueue;
    private Array<Dictionary>[,] _unspawnedWorldObjects;
    private int _worldSpawnThreshold;
    
    public Action<WorldObject> OnWorldObjectAdd { get; set; }
    public Action OnStartAreaLoaded { get; set; }
    public Action<int, int> OnCellLoadStart { get; set; }

    public void Initialize(
        List<(int x, int y)> loadingQueue,
        Array<Dictionary>[,] unspawnedWorldObjects,
        int worldSpawnThreshold
    ) {
        _loadingQueue = loadingQueue;
        _unspawnedWorldObjects = unspawnedWorldObjects;
        _worldSpawnThreshold = worldSpawnThreshold;
        _currentLoadCellCount = 0;
        _isStartAreaLoading = true;
    }

    public override void _Process(double delta) {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        
        while (
            _currentLoadCellCount < _loadingQueue.Count &&
            stopwatch.ElapsedMilliseconds < LoadTimeoutMs
        ) {
            (int x, int y) coords = _loadingQueue[_currentLoadCellCount];
            Array<Dictionary> cellObjects = _unspawnedWorldObjects[coords.x, coords.y];
            
            // Notify that a new cell is being loaded (for initializing the active objects array)
            OnCellLoadStart?.Invoke(coords.x, coords.y);
            
            foreach (Dictionary dictionary in cellObjects) {
                WorldObject worldObject = WorldObject.FromDictionary(dictionary);
                OnWorldObjectAdd?.Invoke(worldObject);
            }

            _currentLoadCellCount++;
        }

        if (_isStartAreaLoading && _currentLoadCellCount >= _worldSpawnThreshold) {
            _isStartAreaLoading = false;
            OnStartAreaLoaded?.Invoke();
        }

        if (_currentLoadCellCount == _loadingQueue.Count) {
            QueueFree();
        }
    }
}

