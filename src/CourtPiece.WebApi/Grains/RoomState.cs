using CourtPiece.Common.Model;

public class RoomState
{
    public List<long> PlayerIds { get; set; } = new();

    public List<Hand> PlayedHands { get; set; } = new();

    public Hand CurrentHand { get; set; } = new();
    public bool IsFinished { get; internal set; }
}

public class Trick
{
    public long? TurnId { get; set; }

    public CardTypes? OriginalSuit { get; set; }

    public Dictionary<long, ICard> Cards { get; set; } = new();
}

public class Hand
{
    public int Number { get; set; }

    //public long TurnId { get; set; }
    public long TrumpCaller { get; set; }

    public Dictionary<long, List<ICard>> PlayerCards { get; set; } = new();

    public CardTypes? TrumpSuit { get; set; }

    public Trick CurrentTrick { get; set; } = new();

    public List<Trick> FirstTeamTricks { get; set; } = new();
    public List<Trick> SecondTeamTricks { get; set; } = new();

    public Winner? Winner { get; set; }
}

public enum Winner
{
    FirstTeam = 1,
    SecondTeam = 2
}


