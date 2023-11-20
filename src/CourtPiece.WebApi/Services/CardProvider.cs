using CourtPiece.Common.Model;

public class CardProvider : ICardProvider
{
    public ICard[] GetCards()
    {
        var cards = new Card[52];

        var j = 0;
        foreach (var cardType in Enum.GetValues<CardTypes>())
        {
            for (var i = 0; i < 13; i++)
            {
                cards[j * 13 + i] = new Card
                {
                    Value = (byte)(i + 1),
                    Type = cardType
                };
            }
            j++;
        }
        var random = new Random();

        return cards.OrderBy(i => random.Next()).ToArray();
    }
}

