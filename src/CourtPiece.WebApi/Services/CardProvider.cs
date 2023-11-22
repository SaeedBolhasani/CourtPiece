using CourtPiece.Common.Model;

public class CardProvider : ICardProvider
{
    public ICard[] GetCards()
    {
        var random = new Random();
        return Card.AllCards.OrderBy(i => random.Next()).ToArray();
    }
}

