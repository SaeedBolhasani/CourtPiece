using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CourtPiece.Common.Model
{
    public interface ICard : IEquatable<ICard>, IComparable<ICard>
    {
        CardTypes Type { get; }
        byte Value { get; }
    }
    public class Card : ICard
    {
        public CardTypes Type { get; private set; }
        public byte Value { get; private set; }

        public Card(CardTypes type, byte value)
        {
            Value = value;
            Type = type;
        }

        private Card() { }

        public static implicit operator Card(string value)
        {
            var parts = value.Split('_');
            return new Card
            {
                Type = (CardTypes)Enum.Parse(typeof(CardTypes), parts[0]),
                Value = byte.Parse(parts[1])
            };
        }

        public static implicit operator string(Card value)
        {
            return value.ToString();
        }

        public override string ToString()
        {
            return $"{Type}_{Value}";
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is Card card)
            {
                return card.Type == Type && card.Value == Value;
            }
            return false;

        }

        public bool Equals(ICard other)
        {
            return Equals(other as object);
        }

        public int CompareTo(ICard other)
        {
            if (Value == 1) return 1;
            if (other.Value == 1) return -1;

            return Value.CompareTo(other.Value);
        }


        public static readonly Card Heart2 = new Card(CardTypes.Hearts, 2);
        public static readonly Card Heart3 = new Card(CardTypes.Hearts, 3);
        public static readonly Card Heart4 = new Card(CardTypes.Hearts, 4);
        public static readonly Card Heart5 = new Card(CardTypes.Hearts, 5);
        public static readonly Card Heart6 = new Card(CardTypes.Hearts, 6);
        public static readonly Card Heart7 = new Card(CardTypes.Hearts, 7);
        public static readonly Card Heart8 = new Card(CardTypes.Hearts, 8);
        public static readonly Card Heart9 = new Card(CardTypes.Hearts, 9);
        public static readonly Card Heart10 = new Card(CardTypes.Hearts, 10);
        public static readonly Card HeartJack = new Card(CardTypes.Hearts, 11);
        public static readonly Card HeartQueen = new Card(CardTypes.Hearts, 12);
        public static readonly Card HeartKing = new Card(CardTypes.Hearts, 13);
        public static readonly Card HeartAce = new Card(CardTypes.Hearts, 1);

        public static readonly Card Spade2 = new Card(CardTypes.Spades, 2);
        public static readonly Card Spade3 = new Card(CardTypes.Spades, 3);
        public static readonly Card Spade4 = new Card(CardTypes.Spades, 4);
        public static readonly Card Spade5 = new Card(CardTypes.Spades, 5);
        public static readonly Card Spade6 = new Card(CardTypes.Spades, 6);
        public static readonly Card Spade7 = new Card(CardTypes.Spades, 7);
        public static readonly Card Spade8 = new Card(CardTypes.Spades, 8);
        public static readonly Card Spade9 = new Card(CardTypes.Spades, 9);
        public static readonly Card Spade10 = new Card(CardTypes.Spades, 10);
        public static readonly Card SpadeJack = new Card(CardTypes.Spades, 11);
        public static readonly Card SpadeQueen = new Card(CardTypes.Spades, 12);
        public static readonly Card SpadeKing = new Card(CardTypes.Spades, 13);
        public static readonly Card SpadeAce = new Card(CardTypes.Spades, 1);

        public static readonly Card Diamond2 = new Card(CardTypes.Diamonds, 2);
        public static readonly Card Diamond3 = new Card(CardTypes.Diamonds, 3);
        public static readonly Card Diamond4 = new Card(CardTypes.Diamonds, 4);
        public static readonly Card Diamond5 = new Card(CardTypes.Diamonds, 5);
        public static readonly Card Diamond6 = new Card(CardTypes.Diamonds, 6);
        public static readonly Card Diamond7 = new Card(CardTypes.Diamonds, 7);
        public static readonly Card Diamond8 = new Card(CardTypes.Diamonds, 8);
        public static readonly Card Diamond9 = new Card(CardTypes.Diamonds, 9);
        public static readonly Card Diamond10 = new Card(CardTypes.Diamonds, 10);
        public static readonly Card DiamondJack = new Card(CardTypes.Diamonds, 11);
        public static readonly Card DiamondQueen = new Card(CardTypes.Diamonds, 12);
        public static readonly Card DiamondKing = new Card(CardTypes.Diamonds, 13);
        public static readonly Card DiamondAce = new Card(CardTypes.Diamonds, 1);


        public static readonly Card Club2 = new Card(CardTypes.Clubs, 2);
        public static readonly Card Club3 = new Card(CardTypes.Clubs, 3);
        public static readonly Card Club4 = new Card(CardTypes.Clubs, 4);
        public static readonly Card Club5 = new Card(CardTypes.Clubs, 5);
        public static readonly Card Club6 = new Card(CardTypes.Clubs, 6);
        public static readonly Card Club7 = new Card(CardTypes.Clubs, 7);
        public static readonly Card Club8 = new Card(CardTypes.Clubs, 8);
        public static readonly Card Club9 = new Card(CardTypes.Clubs, 9);
        public static readonly Card Club10 = new Card(CardTypes.Clubs, 10);
        public static readonly Card ClubJack = new Card(CardTypes.Clubs, 11);
        public static readonly Card ClubQueen = new Card(CardTypes.Clubs, 12);
        public static readonly Card ClubKing = new Card(CardTypes.Clubs, 13);
        public static readonly Card ClubAce = new Card(CardTypes.Clubs, 1);

        public static Card[] AllCards = typeof(Card).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Select(i => i.GetValue(null))
                .OfType<Card>()
                .ToArray();

    }
}
