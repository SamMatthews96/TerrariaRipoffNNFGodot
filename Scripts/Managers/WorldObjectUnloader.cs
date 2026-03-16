using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF;

public partial class WorldObjectUnloader : Node {
    // Limit the number of QueueFree calls per frame to prevent deletion spikes after _Process
    private int _maxQueueFreePerFrame = 300;
    
    private int _currentX;
    private int _currentY;
    private int _width;
    private int _height;
    private string _worldName;
    private (int x, int y) _defaultSpawnPosition;
    
    private Array<WorldObject>[,] _activeWorldObjects;
    private Array<Dictionary>[,] _unspawnedWorldObjects;
    private Array _savedWorldObjects;
    
    public Action OnWorldSaved { get; set; }

    public override void _Ready() {
        ProcessMode = ProcessModeEnum.Always;
    }

    public void Initialize(
        int width,
        int height,
        string worldName,
        (int x, int y) defaultSpawnPosition,
        Array<WorldObject>[,] activeWorldObjects,
        Array<Dictionary>[,] unspawnedWorldObjects
    ) {
        _width = width;
        _height = height;
        _worldName = worldName;
        _defaultSpawnPosition = defaultSpawnPosition;
        _activeWorldObjects = activeWorldObjects;
        _unspawnedWorldObjects = unspawnedWorldObjects;
        _savedWorldObjects = new Array();
        _currentX = 0;
        _currentY = 0;
    }

    public override void _Process(double delta) {
        int queueFreeCount = 0;
        
        while (
            _currentX < _width &&
            queueFreeCount < _maxQueueFreePerFrame
        ) {
            if (_activeWorldObjects[_currentX, _currentY] is null) {
                foreach (Dictionary worldObjectData in _unspawnedWorldObjects[_currentX, _currentY]) {
                    if (worldObjectData["type"].ToString() == "component") continue;
                    _savedWorldObjects.Add(worldObjectData);
                }
            } else {
                Array<WorldObject> cellObjects = _activeWorldObjects[_currentX, _currentY];
                var objectsToProcess = new List<WorldObject>(cellObjects);
                
                foreach (WorldObject worldObject in objectsToProcess) {
                    if (worldObject.Type == "component") continue;
                    if (queueFreeCount >= _maxQueueFreePerFrame) return;
                    
                    _savedWorldObjects.Add(worldObject.ToDictionary());
                    _activeWorldObjects[_currentX, _currentY].Remove(worldObject);
                    worldObject.QueueFree();
                    queueFreeCount++;
                }
            }

            _currentY++;
            if (_currentY >= _height) {
                _currentY = 0;
                _currentX++;
            }
        }

        if (_currentX >= _width) {
            SaveWorldData();
            OnWorldSaved?.Invoke();
            QueueFree();
        }
    }
    
    private void SaveWorldData() {
        Dictionary worldData = new();
        worldData.Add("Name", _worldName);
        worldData.Add("Width", _width);
        worldData.Add("Height", _height);
        worldData.Add("PlayerPositions", new Array());
        worldData.Add("DefaultSpawnPosition",
            new Array { _defaultSpawnPosition.x, _defaultSpawnPosition.y });
        worldData.Add("SavedWorldObjects", _savedWorldObjects);
        
        FileManager.SaveWorld(worldData);
    }
}

