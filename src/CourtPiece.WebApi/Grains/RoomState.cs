using CourtPiece.Common.Model;

public class RoomState
{
    public List<long> PlayerIds { get; set; } = new List<long>();

    public long TurnId { get; set; }
    public long TrumpCaller { get; set; }

    public Dictionary<long, List<Card>> PlayerCards { get; set; } = new();

    public CardTypes? TrumpSuit { get; set; }
    public List<(long playerId, ICard card)> CardsInTrick { get; set; }

    public List<ICard> CurrentTricks { get; set; } = new();

    public List<Trick> FirstTeamWonTricks { get; set; } = new List<Trick>();
    public List<Trick> SecondTeamWonTricks { get; set; } = new List<Trick>();
}

public class Trick
{
    public long TurnId { get; set; }

    public CardTypes BackGround { get; set; }

    public List<ICard> Cards { get; set; } = new();

}


