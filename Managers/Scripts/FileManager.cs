using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class FileManager : Node {
    // C:\Users\Sam-M\AppData\Roaming\Godot\app_userdata\TerrariaRipoffNNF\SavedData

    [Signal]
    public delegate void WorldBasicDataLoadedEventHandler(Dictionary dict);

    public override void _Ready() {
        Array worldBasicDataArray = LoadAllWorldBasicData();
        EmitSignal(SignalName.WorldBasicDataLoaded, worldBasicDataArray);
    }

    private void OnWorldCreatorWorldCreated(World world) {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        Task createWorldTask = Task.Run(() => { SaveWorld(world); });
        createWorldTask.GetAwaiter().OnCompleted(() => {
            watch.Stop();
            GD.Print("world saved in " + watch.ElapsedMilliseconds + " ms");
        });
    }

    private void SaveWorld(World world) {
        EnsureDirectoryExists($"SavedData/worlds/{world.Name}");
        FileAccess fileBasicData = FileAccess.Open(
            $"user://SavedData/worlds/{world.Name}/worldBasicData.txt", FileAccess.ModeFlags.Write);
        WorldBasicInfo worldBasicInfo = world.GetBasicInfo();
        string worldBasicString = worldBasicInfo.Serialize().ToString();
        fileBasicData.StoreString(worldBasicString);
        fileBasicData.Dispose();

        FileAccess file = FileAccess.Open(
            $"user://SavedData/worlds/{world.Name}/world.txt", FileAccess.ModeFlags.Write);
        string worldString = world.Serialize().ToString();
        file.StoreString(worldString);
        file.Dispose();
    }

    private Array LoadAllWorldBasicData() {
        EnsureDirectoryExists("SavedData/worlds");
        DirAccess dirAccess = DirAccess.Open("user://SavedData/worlds");

        string[] directories = dirAccess.GetDirectories();
        Array worldBasicInfos = new();

        for (int i = 0; i < directories.Length; i++) {
            string worldName = directories[i];
            FileAccess fileAccess = FileAccess.Open(
                $"user://SavedData/worlds/{worldName}/worldBasicData.txt", FileAccess.ModeFlags.Read);
            string content = fileAccess.GetAsText();
            fileAccess.Dispose();
            Dictionary myDic = Json.ParseString(content).AsGodotDictionary();
            worldBasicInfos.Add(myDic);
        }

        return worldBasicInfos;
    }

    private void EnsureDirectoryExists(string path) {
        string[] directoryArray = path.Split("/");
        DirAccess dirAccess = DirAccess.Open("user://");

        foreach (var currentFile in directoryArray) {
            if (!dirAccess.DirExists(currentFile)) {
                dirAccess.MakeDir(currentFile);
            }

            dirAccess.ChangeDir(currentFile);
        }
    }
}