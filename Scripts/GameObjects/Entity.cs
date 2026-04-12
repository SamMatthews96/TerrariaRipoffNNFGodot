using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects;

public interface IEntity {
}

public class BlockEntity : IEntity {
    public Vector2I CellCoordinates;
    public string ResourcePath;
    public float CurrentHealth;

}