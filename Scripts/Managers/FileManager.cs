using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public static class FileManager {
    private const string WorldDir = "user://SavedData/worlds";
    // C:\Users\Sam-M\AppData\Roaming\Godot\app_userdata\TerrariaRipoffNNF\SavedData

    public static void SaveWorld(Dictionary worldDictionary) {
        string name = worldDictionary["Name"].ToString();
        int width = worldDictionary["Width"].ToString().ToInt();
        int height = worldDictionary["Height"].ToString().ToInt();
        string worldString = worldDictionary.ToString();

        EnsureDirectoryExists($"{WorldDir}/{name}");
        FileAccess fileBasicData = FileAccess.Open(
            $"{WorldDir}/{name}/worldBasicData.txt", FileAccess.ModeFlags.Write);

        Dictionary worldBasicInfoDictionary = new();
        worldBasicInfoDictionary.Add("Name", name);
        worldBasicInfoDictionary.Add("Width", width);
        worldBasicInfoDictionary.Add("Height", height);

        string worldBasicString = worldBasicInfoDictionary.ToString();
        fileBasicData.StoreString(worldBasicString);
        fileBasicData.Dispose();

        FileAccess file = FileAccess.Open(
            $"{WorldDir}/{name}/world.txt", FileAccess.ModeFlags.Write);
        file.StoreString(worldString);
        file.Dispose();
    }

    public static WorldBasicInfo[] LoadAllWorldBasicData() {
        EnsureDirectoryExists(WorldDir);
        DirAccess dirAccess = DirAccess.Open(WorldDir);

        string[] directories = dirAccess.GetDirectories();
        WorldBasicInfo[] worldBasicInfos = new WorldBasicInfo[directories.Length];

        for (int i = 0; i < directories.Length; i++) {
            string worldName = directories[i];
            FileAccess fileAccess = FileAccess.Open(
                $"{WorldDir}/{worldName}/worldBasicData.txt", FileAccess.ModeFlags.Read);
            string content = fileAccess.GetAsText();
            fileAccess.Dispose();
            Dictionary worldBasicInfoDict = Json.ParseString(content).AsGodotDictionary();

            worldBasicInfos[i] = WorldBasicInfo.FromDict(worldBasicInfoDict);
        }

        return worldBasicInfos;
    }

    public static Dictionary LoadWorld(WorldBasicInfo worldBasicInfo) {
        string worldName = worldBasicInfo.Name;

        FileAccess fileAccess = FileAccess.Open(
            $"{WorldDir}/{worldName}/world.txt", FileAccess.ModeFlags.Read);
        string content = fileAccess.GetAsText();
        fileAccess.Dispose();
        Dictionary worldDict = Json.ParseString(content).AsGodotDictionary();
        return worldDict;
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