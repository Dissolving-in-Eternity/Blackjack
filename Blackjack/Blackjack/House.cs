using Blackjack.Deck;
using System.Collections.Generic;

namespace Blackjack.Blackjack
{
    public class House
    {
        public List<Card> HouseCards { get; private set; }

        public byte Value { get; set; }

        public byte? AlternativeValue { get; set; }

        public bool ShowAllCards { get; set; }

        public House()
        {
            HouseCards = new List<Card>();
            AlternativeValue = 0;
        }
    }
}