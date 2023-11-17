public interface IRoomManager : IGrainWithIntegerKey
{
    Task<IRoom> CreateNewRoom();
    Task<JoinPlayerResult> JoinToRandomRoom(IPlayer player);
    Task<JoinPlayerResult> JoinToRoom(Guid roomId, IPlayer player);
}

