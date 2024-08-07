using System;
using Godot;
using Godot.Collections;
using TerrariaRipoffNNF.Resources.Scripts;

namespace TerrariaRipoffNNF.Managers.Scripts;

public partial class GameManager : Node {

    
    public static GameManager Instance { get; private set; }

    public override void _EnterTree() {
        Instance = this;
    }
    
}