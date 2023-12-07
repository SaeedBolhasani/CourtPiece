using CourtPiece.Common.Model;
using System.Collections;
namespace CourtPiece.IntegrationTest
{
    public partial class RoomTest
    {
        public class GameBuilder : IEnumerable<(Card[], int)>
        {
            private readonly List<(Card[] ThickCards, int WinnerIndex)> cards = new();
            public void Add(Card northPlayerCard, Card westPlayerCard, Card southPlayerCard, Card eastPlayerCard, int winnerPlayerIndex)
            {
                this.cards.Add((new[] { northPlayerCard, westPlayerCard, southPlayerCard, eastPlayerCard }, winnerPlayerIndex));
            }

            public IEnumerable<Card> GetAllCardsOrderedByPlayerIndex()
            {
                for (int i = 0; i < 4; i++)
                    foreach (var (ThickCards, WinnerIndex) in cards)
                    {
                        yield return ThickCards[i];
                    }
            }

            IEnumerator<(Card[], int)> IEnumerable<(Card[], int)>.GetEnumerator()
            {
                return this.cards.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.cards.GetEnumerator();
            }
        }
    }


}