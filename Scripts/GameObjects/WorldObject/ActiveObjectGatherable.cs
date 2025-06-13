using System;

namespace TerrariaRipoffNNF;

public class ActiveObjectGatherable : ActiveObjectProperty {
    public event Action<Player> Gathered;

    public ActiveObjectGatherable(WorldObject worldObject) : base(worldObject) {
        
    }
    
    
    public void GatherAction(Player player) {
        Gathered?.Invoke(player);
    }

    public override void Init() {
        throw new NotImplementedException();
    }
}