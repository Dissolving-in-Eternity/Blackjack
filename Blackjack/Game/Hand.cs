using Blackjack.Deck;
using System.Collections.Generic;

namespace Game
{
    public class Hand
    {
        // Карты на руках у игрока
        public List<Card> HandCards { get; private set; }

        // Сколько очков набрано
        public byte Value { get; set; }

        // На случай тузов
        public byte? AlternativeValue { get; set; }

        public bool IsDoubleDownAvailable { get; set; }

        public bool IsSplitAvailable { get; set; }

        public bool IsFirstDeal { get; set; }

        public bool IsHandOut { get; set; }

        // Ставка в текущем раунде
        public decimal Bet { get; set; }

        public Hand()
        {
            HandCards = new List<Card>();
            AlternativeValue = 0;
            IsFirstDeal = true;
        }
    }
}