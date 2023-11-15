using CourtPiece.Common.Model;

public class RoomState
{
    public List<long> PlayerIds { get; set; } = new List<long>();

    public long TurnId { get; set; }
    public long TrumpCaller { get; set; }

    public List<(long PlayerId, List<Card> cards)> PlayerCards { get; set; } = new();

    public CardTypes? TrumpSuit { get; set; }
    public List<(long playerId, ICard card)> CardsInTrick { get; internal set; }
}


