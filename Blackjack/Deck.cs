using System;
using System.Collections.Generic;
using System.Linq;

namespace Blackjack
{
    public class Deck
    {
        private const int DefaultDeckSize = 52;

        private List<Card> deck;

        private Random random = new Random();

        public Deck()
        {
            deck = new List<Card>(DefaultDeckSize);

            for (int suitIndex = 0; suitIndex < 4; suitIndex++)
                for (int rankIndex = 0; rankIndex < DefaultDeckSize / 4; rankIndex++)
                    deck.Add(new Card((Suit)suitIndex, (Rank)rankIndex));
        }

        public void Shuffle() => 
            deck = deck.OrderBy(card => random.Next()).ToList();
    }
}
