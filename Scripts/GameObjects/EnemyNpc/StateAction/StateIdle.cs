using System;
using Godot;

namespace TerrariaRipoffNNF.Scripts.GameObjects.EnemyNpc;

[GlobalClass]
public partial class StateIdle : StateAction {
    private float _yVelocity;
    private const float GravityCoefficient = 100f;
    
    public override void Enter(EnemyNpc npc) {
        _yVelocity = 0;
    }
    public override void Tick(EnemyNpc npc, double delta) {
        // fall until it hits a block
        bool isFalling = !npc.TestMove(npc.Transform, new Vector2(0, 0.1f));
        if (isFalling) {
            _yVelocity += (float)delta * GravityCoefficient;
        } else {
            _yVelocity = Math.Min(0, _yVelocity);
        }
        
        npc.Velocity = new Vector2(0, _yVelocity);
        npc.MoveAndSlide();
    }
    public override void Exit(EnemyNpc npc) {
    }
}