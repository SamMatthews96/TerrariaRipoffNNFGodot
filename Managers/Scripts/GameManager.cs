using Godot;

namespace TerrariaRipoffNNF.GameManagers.Scripts; 

public abstract partial class GameManager : Node {
    public const int HOST_ID = 1;
    public static GameManager Instance { get; protected set; }
}