using Blackjack.Deck;
using System.Collections.Generic;

namespace Blackjack.Blackjack
{
    public class Hand
    {
        // Карты на руках у игрока
        public List<Card> HandCards { get; private set; }

        // Сколько очков набрано
        public byte Value { get; set; }

        public Hand()
        {
            HandCards = new List<Card>();
        }
    }
}