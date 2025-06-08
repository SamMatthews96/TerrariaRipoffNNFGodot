using System.Collections.Generic;
using Godot;

namespace TerrariaRipoffNNF;

public partial class Data : Node {
    public static PackedScenes PackedScenes { get; private set; }
    public static Recipes Recipes { get; private set; }
    public static Items Items { get; private set; }

    private static readonly Dictionary<string, Resource> LoadedResources = new();

    public static T LoadResource<T>(string path) where T : Resource {
        if (LoadedResources.TryGetValue(path, out Resource resource)) {
            return (T)resource;
        } else {
            T newResource = ResourceLoader.Load<T>(path);
            LoadedResources.Add(path, newResource);
            return newResource;
        }
    }

    [Export] private PackedScenes _packedScenes;
    [Export] private Recipes _recipes;
    [Export] private Items _items;

    public override void _Ready() {
        PackedScenes = _packedScenes;
        Recipes = _recipes;
        Items = _items;
    }
}