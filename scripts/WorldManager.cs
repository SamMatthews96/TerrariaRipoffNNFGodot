using Godot;
using System;
using TerrariaRipoffNNF.scripts;

public partial class WorldManager : Node {
    public static WorldManager Instance { get; private set; }

    [Signal]
    public delegate void CreatedServerWorldManagerEventHandler();

    [Export] private PackedScene packedServerData;
    [Export] public int BlockSize { get; private set; } = 100;
    [Export] private int activeBlockViewDistance = 10;

    public ServerData ServerData { get; private set; }

    public override void _Ready() {
        Instance = this;
    }

    private void OnStartedServer() {
        ServerData = packedServerData.Instantiate<ServerData>();
        AddChild(ServerData);
        EmitSignal(SignalName.CreatedServerWorldManager);
    }

    /*
     * player.LocalPlayerEnteredLocation += (x,y,x,y) => {
     *		delete active blocks that are out of range
     *		get server info of active blocks within range
     * }
     */

    private void OnCreatedLocalPlayer(int xSpawnCoords, int ySpawnCoords) {
        Player.LocalPlayer.LocalPlayerMoved += OnLocalPlayerMoved;
        int peerId = Multiplayer.GetUniqueId();

        RpcId(MultiplayerManager.HOST_ID, nameof(GetSavedBlocksOnServer),
            peerId, xSpawnCoords, ySpawnCoords);
    }

    private void OnLocalPlayerMoved(int xCoords, int yCoords, int prevXCoords, int prevYCoords) {
        int peerId = Multiplayer.GetUniqueId();
        // @todo delete activeBlocks that are out of range
        RpcId(MultiplayerManager.HOST_ID, nameof(GetSavedBlocksOnServer),
            peerId, xCoords, yCoords, prevXCoords, prevYCoords);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void GetSavedBlocksOnServer(int peerId,
        int xCoords, int yCoords, int prevXCoords = int.MaxValue, int prevYCoords = int.MaxValue) {
        // @todo get the new blocks

        bool isOverlappingWithPrevious =
            prevXCoords - xCoords < 2 * activeBlockViewDistance &&
            prevYCoords - yCoords < 2 * activeBlockViewDistance;

        int xViewStart = Math.Max(0, xCoords - activeBlockViewDistance);
        int xViewEnd = Math.Min(ServerData.WorldWidth - 1, xCoords + activeBlockViewDistance);
        int yViewStart = Math.Max(0, yCoords - activeBlockViewDistance);
        int yViewEnd = Math.Min(ServerData.WorldHeight - 1, yCoords + activeBlockViewDistance);
        
        
        
        /*
         * if there is overlap, get start-end x,y of left region,
         * remove blocks in common
         * 
         * for each coord of interest, get server data
         */

        // how to format the savedBlocks data when sending by RPC
        // savedBlock: Resource, check the MustBeVariant tag
        RpcId(peerId, nameof(CreateActiveBlocksOnPeer));
    }

    [Rpc(CallLocal = true)]
    private void CreateActiveBlocksOnPeer() { }
}