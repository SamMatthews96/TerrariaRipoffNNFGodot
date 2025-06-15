using System;

namespace TerrariaRipoffNNF;

public class ObjectGatherable : ObjectProperty {
    public event Action<Player> Gathered;

    public ObjectGatherable(WorldObject worldObject) : base(worldObject) { }


    public void GatherAction(Player player) {
        player.ActionController.GatherAction.OnAfterGatherSuccess();
        Gathered?.Invoke(player);
    }

    public override void Init() { }
}