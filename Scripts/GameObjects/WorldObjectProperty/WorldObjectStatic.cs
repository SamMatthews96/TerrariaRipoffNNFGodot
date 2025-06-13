using System;

namespace TerrariaRipoffNNF;

public partial class WorldObjectStatic : WorldObjectProperty {
    public event Action Gathered;

    public void GatherAction(Player player) {
        float damage = player.PlayerEquipment.Pickaxe.Power;
        if (WorldObject.TryGetProperty(out WorldObjectHealth health)) {
            health.TakeDamage(damage);
        } else {
            Gathered?.Invoke();
        }
    }
    
}