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


    public static readonly JoinPlayerResult Success = new()
    {
        IsSuccess = true,
        ErrorMessage = null,
        Status = GameStatus.NotStarted
    };

    public static readonly JoinPlayerResult GameStarted = new()
    {
        IsSuccess = true,
        ErrorMessage = null,
        Status = GameStatus.Started
    };

    public static JoinPlayerResult Error(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
    };


}

public enum GameStatus
{
    NotStarted = 1,
    Started = 2,
}