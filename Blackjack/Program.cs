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

            bj.CheckCards();

            var handValue = bj.Hands.ElementAt(0).Value;

            if(handValue < 17)
                while (handValue < 17)
                {
                    bj.Hit();

                    handValue = bj.Hands.ElementAt(0).Value;
                }
            else
                bj.Stand();

            bj.CheckValues();
        }
    }
}
