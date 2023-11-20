using Orleans.Providers;

[StorageProvider(ProviderName = StorageNames.DefaultEFStorageName)]
public class RoomManager : Grain<RoomManagerState>, IRoomManager
{
    public async Task<JoinPlayerResult> JoinToNewRoom(IPlayer player)
    {
        var roomId = Guid.NewGuid();
        this.State.EmptyRoomIds.Add(roomId);
        var room = GrainFactory.GetGrain<IRoom>(roomId);

        return await room.JoinPlayer(player);
    }

    public async Task<JoinPlayerResult> JoinToRoom(Guid roomId, IPlayer player)
    {
        var room = GrainFactory.GetGrain<IRoom>(roomId);
        return await room.JoinPlayer(player);
    }

    public async Task<JoinPlayerResult> JoinToRandomRoom(IPlayer player)
    {
        JoinPlayerResult joinPlayerResult;

        if (this.State.EmptyRoomIds.Any())
            joinPlayerResult = await JoinToRoom(this.State.EmptyRoomIds.First(), player);
        else
            joinPlayerResult = await JoinToNewRoom(player);

        if (joinPlayerResult is { IsSuccess: true, Status: GameStatus.Started })
            this.State.EmptyRoomIds.Remove(joinPlayerResult.RoomId);

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

