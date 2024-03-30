using TerrariaRipoffNNF.Utils;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class World {
    private World(string name, int width, int height) {
        Name = name;
        Width = width;
        Height = height;
        SavedBlocks = new SavedBlock[width, height];
    }

    public class Builder {
        private World _world;

        public static Builder New(string name, int width, int height) {
            World world = new(name, width, height);
            return new Builder {
                _world = world
            };
        }

        public static Builder New(WorldBasicInfo worldBasicInfo) {
            World world = new(worldBasicInfo.Name, worldBasicInfo.Width, worldBasicInfo.Height);
            return new Builder {
                _world = world
            };
        }

        public Builder WithSavedBlocks(SavedBlock[,] savedBlocks) {
            _world.SavedBlocks = savedBlocks;
            return this;
        }

        public Builder WithPlayerPosition(string uniqueName, int xPosition, int yPosition) {
            _world.PlayerPositions.Add(uniqueName,
                new GridPosition(xPosition, yPosition));
            return this;
        }

        public World Build() {
            return _world;
        }
    }
}