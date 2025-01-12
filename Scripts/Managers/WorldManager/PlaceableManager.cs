using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlaceableManager : Node {
    [Export] private BlockManager _blockManager;
    private Game _game;

    private SavedPlaceable[,] _savedPlaceableCells;
    private ActivePlaceable[,] _activePlaceableCells;

    public void SetGame(Game game, Dictionary worldData) {
        _game = game;
        _savedPlaceableCells = new SavedPlaceable[_game.Width, _game.Height];
        _activePlaceableCells = new ActivePlaceable[_game.Width, _game.Height];
        Player.PlayerSpawned += OnPlayerManagerPlayerSpawned;
        TreeExiting += OnExiting;
    }

    private void OnExiting() {
        Player.PlayerSpawned -= OnPlayerManagerPlayerSpawned;
        TreeExiting -= OnExiting;
    }

    public bool AreCellsOccupied(IntVector coords, int width, int height) {
        int right = coords.X + width;
        int bottom = coords.Y + height;
        if (right > _game.Width || bottom > _game.Height) return true;

        for (int x = coords.X; x < right; x++) {
            for (int y = coords.Y; y < bottom; y++) {
                if (_savedPlaceableCells[x, y] is not null) return true;
                if (_game.WorldManager.BlockManager.IsCellOccupied(new IntVector(x, y)))
                    return true;
            }
        }

        return false;
    }

    private void OnPlayerManagerPlayerSpawned(Player player) {
        player.ActionController.BuildAction.PlaceablePlaced += OnPlayerPlaceablePlaced;
    }

    private void OnPlayerPlaceablePlaced(Item item, IntVector coords) {
        SavedPlaceable savedPlaceable = SavedPlaceable.Create(item, coords.X, coords.Y);
        foreach (IntVector occupiedCell in savedPlaceable.OccupiedCells) {
            _savedPlaceableCells[occupiedCell.X, occupiedCell.Y] = savedPlaceable;
        }
        SpawnPlaceable(savedPlaceable);
    }

    private void SpawnPlaceable(SavedPlaceable savedPlaceable) {
        ActivePlaceable activePlaceable = ActivePlaceable.Create(savedPlaceable);
        _game.BlockParent.AddChild(activePlaceable, true);
    }
}