public interface IRoomManager : IGrainWithIntegerKey
{
    Task<JoinPlayerResult> JoinToNewRoom(IPlayer player);
    Task<JoinPlayerResult> JoinToRandomRoom(IPlayer player);
    Task<JoinPlayerResult> JoinToRoom(Guid roomId, IPlayer player);
}

