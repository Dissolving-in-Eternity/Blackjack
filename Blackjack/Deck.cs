using System;
using System.Collections.Generic;
using System.Linq;

namespace Blackjack
{
    public class Deck
    {
        private const int DefaultDeckSize = 52;

        public List<Card> DeckOfCards { get; set; }

        private Random random = new Random();

        public Deck()
        {
            DeckOfCards = new List<Card>(DefaultDeckSize);

            for (int suitIndex = 0; suitIndex < 4; suitIndex++)
                for (int rankIndex = 0; rankIndex < DefaultDeckSize / 4; rankIndex++)
                    DeckOfCards.Add(new Card((Suit)suitIndex, (Rank)rankIndex));
        }

        public void Shuffle() => 
            DeckOfCards = DeckOfCards.OrderBy(card => random.Next()).ToList();
    }
}
