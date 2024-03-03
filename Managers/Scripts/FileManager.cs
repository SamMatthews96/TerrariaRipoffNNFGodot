using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class FileManager : Node {
    private const string ROOT_FOLDER = "user://SavedData/";

    private void OnWorldCreated(World world) {
        GD.Print("world created in fileman");
        GD.Print(world.Name);
        GD.Print(world.WorldWidth);
        GD.Print(world.WorldHeight);
    }
    
    private void SaveWorld() {
        /*
         * SavedData/Worlds/WorldId
         */
    }

    private void LoadWorldsBasicData() {
        
    }

    private void LoadWorld(string path) {
        
    }

}
