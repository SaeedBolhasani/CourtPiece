using Orleans.Providers;

[StorageProvider(ProviderName = "File")]
public class RoomManager : Grain<RoomManagerState>, IRoomManager
{
    public Task<IRoom> CreateNewRoom()
    {
        var roomId = Guid.NewGuid();
        this.State.EmptyRoomIds.Add(roomId);
        return Task.FromResult(GrainFactory.GetGrain<IRoom>(roomId));
    }

    public async Task<JoinPlayerResult> JoinToRoom(Guid roomId, IPlayer player)
    {
        var room = GrainFactory.GetGrain<IRoom>(roomId);
        return await room.JoinPlayer(player);
    }

    public async Task<JoinPlayerResult> JoinToRandomRoom(IPlayer player)
    {
        IRoom room;

        if (this.State.EmptyRoomIds.Any())
            room = GrainFactory.GetGrain<IRoom>(this.State.EmptyRoomIds.First());
        else
            room = await CreateNewRoom();

        var joinPlayerResult = await room.JoinPlayer(player);

        if (joinPlayerResult == JoinPlayerResult.GameStarted)
            this.State.EmptyRoomIds.Remove(room.GetPrimaryKey());

        return joinPlayerResult;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await ReadStateAsync();
        await base.OnActivateAsync(cancellationToken);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        await WriteStateAsync();
        await base.OnDeactivateAsync(reason, cancellationToken);
    }
}

