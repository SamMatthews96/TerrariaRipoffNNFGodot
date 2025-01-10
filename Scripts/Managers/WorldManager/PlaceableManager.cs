using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class PlaceableManager : Node {
    /* Handles placement of non block objects like
     Furniture, torches, crafting stations, etc
     
     Will need to listen to player actions for placing and mining objects
     Placed objects will need coordinates and size
     When trying to place, this will need to check with the blockManager to see if 
     any blocks are in the way.
     */
    [Export] private BlockManager _blockManager;
    private SavedPlaceable[,] _savedPlaceableCells;
    private Game _game;

    public void SetGame(Game game, Dictionary worldData) {
        _game = game;
        Player.PlayerSpawned += OnPlayerManagerPlayerSpawned;
        _savedPlaceableCells = new SavedPlaceable[_game.Width, _game.Height];
        
    }
    
    public bool AreCellsOccupied(IntVector coords, int width, int height) {
        int right = Math.Min(coords.X + width, _game.Width - 1);
        int bottom = Math.Min(coords.Y + height, _game.Height - 1);
        for (int x = coords.X; x < right; x++) {
            for (int y = coords.Y; y < bottom; y++) {
                if (_savedPlaceableCells[x, y] is not null) return true;
            }
        }
        return false;
    }

    private void OnPlayerManagerPlayerSpawned(Player player) {
        // player.ActionController..PlaceablePlaced += OnPlayerPlaceablePlaced;
    }
}