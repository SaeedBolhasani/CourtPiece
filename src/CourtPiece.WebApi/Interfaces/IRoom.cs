using CourtPiece.Common.Model;
using Orleans.Concurrency;

public interface IRoom: IGrainWithGuidKey
{
    Task AddPlayer(IPlayer player);
    Task Action(Immutable<ICard> card, IPlayer player);
}