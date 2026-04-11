using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public interface IEntity {
}

public struct BlockEntity : IEntity {
    public Vector2I CellCoordinates;
    public string ResourcePath;
    public float CurrentHealth;

}