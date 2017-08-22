using Blackjack.Deck;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace Game
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var bj = new Blackjack(500);

            WriteLine("Welcome to the Blackjack by Pride!");

            do
            {
                if (Blackjack.IsRoundFinished)
                {
                    var moneyFromTheLastRound = bj.Money;
                    bj = new Blackjack(moneyFromTheLastRound);

                    Blackjack.IsRoundFinished = false;
                }

                bj.GameEnd += PrintGameOutcome;

                WriteLine($"\nYour current amount of money is { bj.Money }.\n");

                PlaceABet(bj);

                bj.FirstDeal();

                bj.BlackjackCheckInit();

                // Если блекджек не случился
                if (!Blackjack.IsRoundFinished)
                    PerformUserAction(bj);

                // Если после действий пользователя раунд всё ещё не окончен
                if (!Blackjack.IsRoundFinished)
                    bj.CheckValues(true);

                WriteLine(bj.Money > 0
                    ? "\nPress 'Enter' to continue to the next round or 'Esc' to exit."
                    : "\nYou're out of money! Game over.");
            } while (ReadKey().Key != ConsoleKey.Escape && bj.Money > 0);
        }

        private static void PerformUserAction(Blackjack bj)
        {
            //Dictionary<ConsoleKey, Action> actions = 
            //    new Dictionary<ConsoleKey, Action>
            //    {
            //        { ConsoleKey.D1, bj.Hit },
            //        { ConsoleKey.D2, bj.Stand },
            //        { ConsoleKey.D3, bj.DoubleDown }
            //    };

            ConsoleKeyInfo action;

            do
            {
                if(Blackjack.IsRoundFinished)
                    break;

                var isDoubleDownAvaliable = 
                    bj.Hands.ElementAt(0).HandCards.Count == 2 && bj.Money > bj.CurrentBet;

                if (bj.House.ShowAllCards)
                    PrintCardsInfo(bj.Hands, bj.House, true);
                else
                    PrintCardsInfo(bj.Hands, bj.House);

                WriteLine("\n1. Hit");
                WriteLine("2. Stand");
                if(isDoubleDownAvaliable)
                    WriteLine("3. Double Down");
                Write("\nYour action: ");
                action = ReadKey();
                WriteLine();

                if (action.Key != ConsoleKey.D1 && action.Key != ConsoleKey.D2 && action.Key != ConsoleKey.D3)
                {
                    WriteLine("\nWrong character.");
                    continue;
                }

                if (action.Key == ConsoleKey.D1)
                    bj.Hit();
                else if (action.Key == ConsoleKey.D2)
                    bj.Stand();
                else
                {
                    if(isDoubleDownAvaliable)
                    {
                        bj.DoubleDown();

                        // Обеспечиваем условие последней взятой карты за раунд
                        if(!Blackjack.IsRoundFinished)
                            bj.Stand();

                        break;
                    }

                    WriteLine("Action not avaliable");
                }
            }
            while (action.Key != ConsoleKey.D2);
        }

        private static void PrintGameOutcome(string s, decimal m, List<Hand> hands, House house)
        {
            WriteLine("\n\nRound finished with the following outcome:");
            PrintCardsInfo(hands, house, true);
            Write("\n" + s);
            WriteLine($"{m}$\n");

            Blackjack.IsRoundFinished = true;
        }

        private static void PlaceABet(Blackjack bj)
        {
            while (true)
            {
                try
                {
                    Write("\nPlace a bet: ");
                    var bet = decimal.Parse(ReadLine());

                    if (bet <= 0)
                    {
                        WriteLine("Bet can't be less than or equal to 0.");
                        continue;
                    }

                    if (bet > bj.Money)
                    {
                        WriteLine("Not enough money.");
                        continue;
                    }

                    bj.CurrentBet = bet;

                    break;
                }
                catch (FormatException)
                {
                    WriteLine("You must enter a whole number.");
                }
            }
        }

        private static void PrintCardsInfo(List<Hand> hands, House house, bool showAllCards = false)
        {
            PrintUserCards(hands);
            PrintHouseCards(house, showAllCards);
        }

        private static void PrintUserCards(List<Hand> hands)
        {
            var hand = hands.ElementAt(0);

            WriteLine($"\nYour cards ({ hand.Value }" +
                      $"{ (hand.AlternativeValue != null ? "/" + hand.AlternativeValue : string.Empty) }):");
            PrintCards(hand.HandCards);
        }

        private static void PrintHouseCards(House house, bool showAllCards)
        {
            if (!showAllCards)
            {
                WriteLine("\nHouse cards:");
                WriteLine(" - " + house.HouseCards.First());
                WriteLine(" - [CLOSED]");
            }
            else
            {
                WriteLine($"\nHouse cards ({ house.Value }" +
                          $"{ (house.AlternativeValue != null ? "/" + house.AlternativeValue : string.Empty) }):");

                PrintCards(house.HouseCards);
            }
        }

        private static void PrintCards(List<Card> cards)
        {
            foreach (var card in cards)
                WriteLine(" - " + card);
        }
    }
}