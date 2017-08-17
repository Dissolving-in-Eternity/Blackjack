using Blackjack.Deck;
using System.Collections.Generic;

namespace Blackjack.Blackjack
{
    // Singleton
    public class House
    {
        public List<Card> HouseCards { get; private set; }
        public byte HouseValue { get; set; }

        private static House house;
        private House() { HouseCards = new List<Card>(); }

        public static House GetHouse
        {
            get
            {
                if(house == null)
                    house = new House();
                return house;
            }
        }
    }
}