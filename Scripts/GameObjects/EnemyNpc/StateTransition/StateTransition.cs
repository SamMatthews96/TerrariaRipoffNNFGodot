using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

[GlobalClass]
public abstract partial class StateTransition : Resource {
    [Export] public string To { get; private set; }
    public abstract bool ShouldTransition(EnemyNpc npc);
}