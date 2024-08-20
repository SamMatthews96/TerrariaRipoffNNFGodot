using System;
using System.Collections.Generic;
using Godot;
using TerrariaRipoffNNF.Scripts.GameObjects;
using TerrariaRipoffNNF.Scripts.UI;

namespace TerrariaRipoffNNF.Scripts.Actions;

public class BuildActionState : IActionState {
    private Player _player;

    public static event Action OnBuildActionEquipped;
    public static event Action OnBuildActionUnequipped;
    
    public BuildActionState(Player player) {
        _player = player;
    }

    public void Equip() {
        OnBuildActionEquipped?.Invoke();
    }
    
    public void Unequip() {
        OnBuildActionUnequipped?.Invoke();
    }

    public void PrimaryAction(Vector2 mouseWorldPosition) {
        
    }

    public void EndPrimaryAction(Vector2 mouseScreenPosition) {
        
    }
}