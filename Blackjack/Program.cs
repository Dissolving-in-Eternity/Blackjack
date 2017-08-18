using System.Linq;

namespace Blackjack
{
    class Program
    {
        static void Main(string[] args)
        {
            var bj = new Blackjack.Blackjack(5000);
            bj.CurrentBet = 500;

            bj.FirstDeal();

            bj.BlackjackCheckInit();

            var handValue = bj.Hands.ElementAt(0).Value;
            var handAltValue = bj.Hands.ElementAt(0).AlternativeValue;

            while (handValue < 17 || handAltValue != null && handAltValue < 18)
            {
                bj.Hit();

                handValue = bj.Hands.ElementAt(0).Value;
                handAltValue = bj.Hands.ElementAt(0).AlternativeValue;
            }

            bj.Stand();

            bj.CheckValues(true);
        }
    }
}
