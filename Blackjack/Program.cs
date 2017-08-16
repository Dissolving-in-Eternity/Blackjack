namespace Blackjack
{
    class Program
    {
        static void Main(string[] args)
        {
            Blackjack bj = new Blackjack(5000);
            bj.CurrentBet = 500;
            bj.Deal();
            bj.CardsCheck();
        }
    }
}
