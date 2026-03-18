using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public struct Entity {
    public Vector2 Position;
    public EntityType Type;
    public IEntityComponent Component;
}

public enum EntityType {
    Block, 
    Wall
}

public interface IEntityComponent {
}

public struct EntityBlock : IEntityComponent {
    public string ResourcePath;
    public float CurrentHealth;
    
}