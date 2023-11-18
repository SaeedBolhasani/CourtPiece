using CourtPiece.Common.Model;
using Orleans.Concurrency;

public interface IRoom : IGrainWithGuidKey
{
    Task<JoinPlayerResult> JoinPlayer(IPlayer player);
    Task Action(Immutable<ICard> card, IPlayer player);
}


[GenerateSerializer]
public class JoinPlayerResult
{
    [Id(0)]
    public bool IsSuccess { get; private set; }

    [Id(1)]
    public string? ErrorMessage { get; private set; }

    [Id(2)]
    public GameStatus Status { get; private set; }

    [Id(3)]
    public Guid RoomId { get; private set; }

    public static JoinPlayerResult Success(Guid roomId) => new()
    {
        IsSuccess = true,
        ErrorMessage = null,
        RoomId = roomId,
        Status = GameStatus.NotStarted
    };

    public static JoinPlayerResult GameStarted(Guid roomId) => new()
    {
        IsSuccess = true,
        ErrorMessage = null,
        RoomId = roomId,
        Status = GameStatus.Started
    };

    public static JoinPlayerResult Error(string message, Guid roomId) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        RoomId = roomId,
    };

}

public enum GameStatus
{
    NotStarted = 1,
    Started = 2,
}