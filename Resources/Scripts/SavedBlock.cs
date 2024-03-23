using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Resources.Scripts;

public partial class SavedBlock : Resource {
    private float currentHealth;

    [Signal]
    public delegate void DestroyedEventHandler(int xPosition, int yPosition);

    public int XPosition { get; }
    public int YPosition { get; }
    public BlockType BlockType { get; }

    public SavedBlock(BlockType blockType, int xPosition, int yPosition) {
        BlockType = blockType;
        XPosition = xPosition;
        YPosition = yPosition;
        currentHealth = blockType.MaxHealth;
    }

    public void TakeDamage(float damageAmount) {
        currentHealth -= damageAmount;
        if (currentHealth <= 0) {
            EmitSignal(SignalName.Destroyed, XPosition, YPosition);
        }
    }

    public Dictionary Serialize() {
        Dictionary serializedData = new();
        serializedData.Add("XPosition", XPosition);
        serializedData.Add("YPosition", YPosition);
        serializedData.Add("ResourcePath", BlockType.ResourcePath);
        return serializedData;
    }

    public static SavedBlock FromDict(Dictionary dictionary) {
        return new SavedBlock(
            ResourceLoader.Load<BlockType>(dictionary["ResourcePath"].ToString()),
            dictionary["XPosition"].ToString().ToInt(),
            dictionary["YPosition"].ToString().ToInt()
            );
    }
}