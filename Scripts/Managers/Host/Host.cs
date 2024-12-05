using System;
using Godot;
using Godot.Collections;

namespace TerrariaRipoffNNF;

public partial class Host : Node {
    [Export] public PlayerManager PlayerManager { get; private set; }
    [Export] public BlockManager BlockManager { get; private set; }
    [Export] public PickupManager PickupManager { get; private set; }

    public static void RequireHost() {
        if (!Manager.Instance.Game.IsHost) {
            throw new Exception("[20240813.1408.1] Method should only be called on the host");
        }
    }

    public override void _EnterTree() {
        RequireHost();
    }

    // @todo initialisers
    public void Initialize(Dictionary worldDictionary) {
        PlayerManager.Initialize(worldDictionary);
        BlockManager.Initialize(worldDictionary);
    }
}