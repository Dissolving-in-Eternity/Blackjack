using Blackjack.Cards;
using System.Collections.Generic;

namespace Blackjack.Game
{
    public class House
    {
        public List<Card> HouseCards { get; private set; }

        public byte Value { get; set; }

        public byte? AlternativeValue { get; set; }

        public const byte DrawTo = 17;

        public House()
        {
            HouseCards = new List<Card>();
            AlternativeValue = 0;
        }
    }
}