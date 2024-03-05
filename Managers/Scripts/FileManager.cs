using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class FileManager : Node {
    private const string ROOT_FOLDER = "user://SavedData";
    // C:\Users\Sam-M\AppData\Roaming\Godot\app_userdata\TerrariaRipoffNNF\SavedData

    public override void _Ready() {
        
    }

    private void OnWorldCreated(World world) {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        Task createWorldTask = Task.Run(() => {
            SaveWorld(world);
        });
        createWorldTask.GetAwaiter().OnCompleted(() => {
            watch.Stop();
            GD.Print("world saved in " + watch.ElapsedMilliseconds + " ms");
        });
    }
    
    private void SaveWorld(World world) {
        string path = $"{ROOT_FOLDER}/worlds/{world.Name}";
        EnsureDirectoryExists(path);

        FileAccess fileBasicData = FileAccess.Open(
            $"{path}/worldBasicData.txt", FileAccess.ModeFlags.Write);
        WorldBasicInfo worldBasicInfo = world.GetBasicInfo();
        string worldBasicString = worldBasicInfo.Serialize().ToString();
        fileBasicData.StoreString(worldBasicString);
        fileBasicData.Dispose();
        
        FileAccess file = FileAccess.Open($"{path}/world.txt", FileAccess.ModeFlags.Write);
        string worldString = world.Serialize().ToString();
        file.StoreString(worldString);
        file.Dispose();
    }

    private void EnsureDirectoryExists(string path) {
        string[] directoryArray = path.Split("/");
        DirAccess dirAccess = DirAccess.Open(ROOT_FOLDER);
        
        foreach (var currentFile in directoryArray) {
            if (!dirAccess.DirExists(currentFile)) {
                dirAccess.MakeDir(currentFile);
            }
            dirAccess.ChangeDir(currentFile);
        }
    }

    private WorldBasicInfo[] LoadAllWorldBasicData() {
        throw new NotImplementedException();
    }
    
}


