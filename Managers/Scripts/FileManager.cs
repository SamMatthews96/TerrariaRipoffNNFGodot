using Godot;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class FileManager : Node {
    private const string ROOT_FOLDER = "user://";
    // C:\Users\Sam-M\AppData

    private void OnWorldCreated(World world) {
        SaveWorld(world);
    }
    
    private void SaveWorld(World world) {
        GD.Print("start save");
        string directory = $"{ROOT_FOLDER}/worlds/{world.Name}";
        
        DirAccess dir = DirAccess.Open(ROOT_FOLDER);
        if (!dir.DirExists("worlds")) {
            dir.MakeDir("worlds");
        }
        dir.ChangeDir("worlds");
        
        if (!dir.DirExists(world.Name)) {
            dir.MakeDir(world.Name);
        }
        dir.ChangeDir(world.Name);
        
        if (!FileAccess.FileExists($"{directory}/world.tres")) {
            FileAccess.Open(directory, FileAccess.ModeFlags.Write);
        }
        ResourceSaver.Save(world, $"{directory}/world.tres");
        
        // WorldBasicInfo worldBasicInfo = new WorldBasicInfo(world.Name,world.WorldWidth,world.WorldHeight);
        // if (!FileAccess.FileExists($"{directory}/worldBasicInfo.tres")) {
        //     DirAccess.Open(directory);
        //     FileAccess.Open(directory, FileAccess.ModeFlags.Write);
        //     ResourceSaver.Save(worldBasicInfo, $"{directory}/worldBasicInfo.tres");
        // }
        
        GD.Print("end save");
    }

    private void LoadWorldsBasicData() {
        
    }

    private void LoadWorld(string path) {
        
    }



}
