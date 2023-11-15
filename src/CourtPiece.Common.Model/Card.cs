using System;

namespace CourtPiece.Common.Model
{
    public interface ICard:IEquatable<ICard>
    {
        CardTypes Type { get; }
        byte Value { get; }
    }
    public class Card : ICard
    {
        public CardTypes Type { get; set; }
        public byte Value { get; set; }

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
            if(obj is Card card)
            {
                return card.Type == Type && card.Value == Value;
            }
            return false;

        }

        public bool Equals(ICard other)
        {
            return Equals(other as object);
        }
    }
}
