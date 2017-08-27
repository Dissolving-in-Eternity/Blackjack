using System;
using System.Collections.Generic;
using System.Linq;

namespace Blackjack.Cards
{
    public class Deck
    {
        private const int DefaultDeckSize = 52;

        public List<Card> DeckOfCards { get; set; }

        private readonly Random random = new Random();

        public Deck()
        {
            DeckOfCards = new List<Card>(DefaultDeckSize);

            for (var suitIndex = 0; suitIndex < 4; suitIndex++)
                for (var rankIndex = 0; rankIndex < DefaultDeckSize / 4; rankIndex++)
                    DeckOfCards.Add(new Card((Suit)suitIndex, (Rank)rankIndex));
        }

        public void Shuffle() => 
            DeckOfCards = DeckOfCards.OrderBy(card => random.Next()).ToList();
    }
}
