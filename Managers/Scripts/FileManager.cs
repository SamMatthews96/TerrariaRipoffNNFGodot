using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;
using Array = Godot.Collections.Array;

namespace TerrariaRipoffNNF.Managers.Scripts;

public static class FileManager {
    private const string WORLD_DIR = "user://SavedData/worlds";
    // C:\Users\Sam-M\AppData\Roaming\Godot\app_userdata\TerrariaRipoffNNF\SavedData

    public static void SaveWorld(World world) {
        EnsureDirectoryExists($"{WORLD_DIR}/{world.Name}");
        FileAccess fileBasicData = FileAccess.Open(
            $"{WORLD_DIR}/{world.Name}/worldBasicData.txt", FileAccess.ModeFlags.Write);
        WorldBasicInfo worldBasicInfo = world.GetBasicInfo();
        string worldBasicString = worldBasicInfo.Serialize().ToString();
        fileBasicData.StoreString(worldBasicString);
        fileBasicData.Dispose();

        FileAccess file = FileAccess.Open(
            $"{WORLD_DIR}/{world.Name}/world.txt", FileAccess.ModeFlags.Write);
        string worldString = world.Serialize().ToString();
        file.StoreString(worldString);
        file.Dispose();
    }

    public static WorldBasicInfo[] LoadAllWorldBasicData() {
        EnsureDirectoryExists(WORLD_DIR);
        DirAccess dirAccess = DirAccess.Open(WORLD_DIR);

        string[] directories = dirAccess.GetDirectories();
        WorldBasicInfo[] worldBasicInfos = new WorldBasicInfo[directories.Length];

        for (int i = 0; i < directories.Length; i++) {
            string worldName = directories[i];
            FileAccess fileAccess = FileAccess.Open(
                $"{WORLD_DIR}/{worldName}/worldBasicData.txt", FileAccess.ModeFlags.Read);
            string content = fileAccess.GetAsText();
            fileAccess.Dispose();
            Dictionary worldBasicInfoDict = Json.ParseString(content).AsGodotDictionary();

            worldBasicInfos[i] = WorldBasicInfo.FromDict(worldBasicInfoDict);
        }

        return worldBasicInfos;
    }

    public static World LoadWorld(WorldBasicInfo worldBasicInfo) {
        string worldName = worldBasicInfo.Name;

        FileAccess fileAccess = FileAccess.Open(
            $"{WORLD_DIR}/{worldName}/worldBasicData.txt", FileAccess.ModeFlags.Read);
        string content = fileAccess.GetAsText();
        fileAccess.Dispose();
        Dictionary worldDict = Json.ParseString(content).AsGodotDictionary();
        return World.FromDict(worldDict);
    }

    private static void EnsureDirectoryExists(string path) {
        string relativePath = path.Replace("user://", "");
        string[] directoryArray = relativePath.Split("/");
        DirAccess dirAccess = DirAccess.Open("user://");

        foreach (var currentFile in directoryArray) {
            if (!dirAccess.DirExists(currentFile)) {
                dirAccess.MakeDir(currentFile);
            }

            dirAccess.ChangeDir(currentFile);
        }
    }
}