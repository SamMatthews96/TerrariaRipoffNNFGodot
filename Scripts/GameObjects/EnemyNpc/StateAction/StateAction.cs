using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

[GlobalClass]
public abstract partial class StateAction : Resource {
    [Export] private Array<StateTransition> _transitions = new();
    public abstract void Enter(EnemyNpc npc);

    public abstract void Tick(EnemyNpc npc, double delta);

    public abstract void Exit(EnemyNpc npc);

    public string GetTransition(EnemyNpc npc) {
        foreach (StateTransition transition in _transitions) {
            if (transition.ShouldTransition(npc)) {
                return transition.To;
            }
        }
        return null;
    }
}