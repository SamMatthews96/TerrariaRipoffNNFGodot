using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class ActiveBlock : ActiveWorldObject {
    public SavedBlock SavedBlock { get; private set; }
   
    [Export] private Sprite2D _sprite;

    public override void _Ready() {
        SavedBlock = SavedBlock.FromDictionary(ObjectConfig);

        Position = new Vector2(
            SavedBlock.XPosition * Game.BlockSize,
            SavedBlock.YPosition * Game.BlockSize);
        _sprite.Texture = SavedBlock.Item.GetProperty<ItemBlock>().Texture;
    }
    
    public static ActiveBlock Create(SavedBlock savedBlock) {
        ActiveBlock activeBlock = Data.PackedScenes.ActiveBlock.Instantiate<ActiveBlock>();
        activeBlock.ObjectConfig = savedBlock.ToDictionary();
        
        return activeBlock;
    }
}