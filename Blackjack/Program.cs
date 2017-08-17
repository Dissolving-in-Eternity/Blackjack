namespace Blackjack
{
    class Program
    {
        static void Main(string[] args)
        {
            Blackjack.Blackjack bj = new Blackjack.Blackjack(5000);
            bj.CurrentBet = 500;
            bj.Deal(true);
            bj.CardsCheck();
        }
    }
}
