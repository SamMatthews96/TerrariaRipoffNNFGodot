using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public static class FileManager {
    private const string WORLD_DIR = "user://SavedData/worlds";
    // C:\Users\Sam-M\AppData\Roaming\Godot\app_userdata\TerrariaRipoffNNF\SavedData

    private static readonly Dictionary<string, BlockType> LoadedBlockTypes = new();

    public static void SaveWorld(Dictionary worldDictionary) {
        try {
            string name = worldDictionary["Name"].ToString();
            int width = worldDictionary["Width"].ToString().ToInt();
            int height = worldDictionary["Height"].ToString().ToInt();
            string worldString = worldDictionary.ToString();

            EnsureDirectoryExists($"{WORLD_DIR}/{name}");
            FileAccess fileBasicData = FileAccess.Open(
                $"{WORLD_DIR}/{name}/worldBasicData.txt", FileAccess.ModeFlags.Write);

            Dictionary worldBasicInfoDictionary = new();
            worldBasicInfoDictionary.Add("Name", name);
            worldBasicInfoDictionary.Add("Width", width);
            worldBasicInfoDictionary.Add("Height", height);

            string worldBasicString = worldBasicInfoDictionary.ToString();
            fileBasicData.StoreString(worldBasicString);
            fileBasicData.Dispose();

            FileAccess file = FileAccess.Open(
                $"{WORLD_DIR}/{name}/world.txt", FileAccess.ModeFlags.Write);
            file.StoreString(worldString);
            file.Dispose();
        }
        catch (Exception e) {
            GD.PrintErr("Error saving world");
            GD.PrintErr(e.Message);
            throw new NotImplementedException();
        }
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
            $"{WORLD_DIR}/{worldName}/world.txt", FileAccess.ModeFlags.Read);
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

    public static BlockType LoadBlockType(string resourcePath) {
        if (LoadedBlockTypes.TryGetValue(resourcePath, out BlockType type)) {
            return type;
        }

        BlockType blockType = ResourceLoader.Load<BlockType>(resourcePath);
        LoadedBlockTypes.Add(resourcePath, blockType);
        return blockType;
    }
}