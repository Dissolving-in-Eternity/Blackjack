using Blackjack.Cards;
using System.Collections.Generic;

namespace Blackjack.Game
{
    public class Hand
    {
        public List<Card> HandCards { get; private set; }

        // How many points user have for current hands
        public byte Value { get; set; }

        // In case of Aces
        public byte? AlternativeValue { get; set; }

        public bool IsDoubleDownAvailable { get; set; }

        public bool IsSplitAvailable { get; set; }

        public bool IsFirstDeal { get; set; }

        public bool IsHandOut { get; set; }

        // Current round bet
        public decimal Bet { get; set; }

        public Hand()
        {
            HandCards = new List<Card>();
            AlternativeValue = 0;
            IsFirstDeal = true;
        }
    }
}