using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;

namespace TerrariaRipoffNNF;

public class BlockEntity : IEntity {
    public Vector2I CellCoordinates;
    public string ResourcePath;
    public float CurrentHealth;
}