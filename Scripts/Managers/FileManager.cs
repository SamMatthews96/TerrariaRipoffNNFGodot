using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public static class FileManager {
    private const string WorldDir = "user://SavedData/worlds";
    private const string PlayerDir = "user://SavedData/players";
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
    
    public static void DeleteWorld(WorldBasicInfo worldBasicInfo) {
        string worldName = worldBasicInfo.Name;
        DirAccess dirAccess = DirAccess.Open(WorldDir);
        dirAccess.Remove($"{worldName}/world.txt");
        dirAccess.Remove($"{worldName}/worldBasicData.txt");
        dirAccess.Remove(worldName);
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
    
    public static void SavePlayer(Dictionary playerDictionary) {
        string name = playerDictionary["Name"].ToString();
        string playerString = playerDictionary.ToString();

        EnsureDirectoryExists($"{PlayerDir}/{name}");
        FileAccess file = FileAccess.Open(
            $"{PlayerDir}/{name}/playerBasicData.txt", FileAccess.ModeFlags.Write);
        file.StoreString(playerString);
        file.Dispose();
    }

    public static Dictionary[] LoadAllPlayerBasicData() {
        EnsureDirectoryExists(PlayerDir);
        DirAccess dirAccess = DirAccess.Open(PlayerDir);
        
        string[] directories = dirAccess.GetDirectories();
        Dictionary[] playerBasicInfos = new Dictionary[directories.Length];
        
        for (int i = 0; i < directories.Length; i++) {
            string playerName = directories[i];
            FileAccess fileAccess = FileAccess.Open(
                $"{PlayerDir}/{playerName}/playerBasicData.txt", FileAccess.ModeFlags.Read);
            string content = fileAccess.GetAsText();
            fileAccess.Dispose();
            Dictionary playerBasicInfo = Json.ParseString(content).AsGodotDictionary();
            playerBasicInfos[i] = playerBasicInfo;
        }

        return playerBasicInfos;
    }
    
    public static void DeletePlayer(Dictionary playerBasicInfo) {
        string playerName = playerBasicInfo["Name"].ToString();
        DirAccess dirAccess = DirAccess.Open(PlayerDir);
        dirAccess.Remove($"{playerName}/playerBasicData.txt");
        dirAccess.Remove(playerName);
    }
    
    public static Dictionary LoadPlayer(Dictionary playerData) {
        string playerName = playerData["Name"].ToString();
        FileAccess fileAccess = FileAccess.Open(
            $"{PlayerDir}/{playerName}/playerBasicData.txt", FileAccess.ModeFlags.Read);
        string content = fileAccess.GetAsText();
        fileAccess.Dispose();
        Dictionary playerDict = Json.ParseString(content).AsGodotDictionary();
        return playerDict;
    }
}