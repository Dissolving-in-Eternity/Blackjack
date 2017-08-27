using System.Linq;

namespace Blackjack.Game
{
    public static class GameHandler
    {
        public static void Init()
        {
            var bj = new Game.Blackjack(500);

            ConsoleHelper.Greetings();

            do
            {
                NewRound(bj);
            } while (!ConsoleHelper.Exit() && bj.Money > 0);
        }

        private static void NewRound(Blackjack bj)
        {
            if (bj.Hands.All(h => h.IsHandOut))
            {
                var moneyFromTheLastRound = bj.Money;
                bj = new Game.Blackjack(moneyFromTheLastRound);

                bj.CurHand.IsHandOut = false;
            }

            bj.Endgame += ConsoleHelper.PrintGameOutcome;

            ConsoleHelper.PrintMoneyAmount(bj.Money);

            ConsoleHelper.PlaceABet(bj);

            bj.FirstDeal();

            ConsoleHelper.PerformUserAction(bj);

            ConsoleHelper.PrintEndRoundOptions(bj.Money);
        }
    }
}
