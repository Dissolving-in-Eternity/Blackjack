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
            var actions =
                new Dictionary<ConsoleKey, Action>
                {
                    { ConsoleKey.D1, bj.Hit },
                    { ConsoleKey.D2, bj.Stand },
                    { ConsoleKey.D3, bj.DoubleDown }
                };

            ConsoleKey action;

            do
            {
                if(Blackjack.IsRoundFinished)
                    break;

                PrintCardsInfo(bj.Hands, bj.House);

                WriteLine("\n1. Hit");
                WriteLine("2. Stand");
                if(Blackjack.IsDoubleDownAvailable)
                    WriteLine("3. Double Down");
                Write("\nYour action: ");
                action = ReadKey().Key;
                WriteLine();

                if(!actions.ContainsKey(action))
                {
                    WriteLine("\nWrong character.");
                    continue;
                }

                Action todo;
                actions.TryGetValue(action, out todo);

                if (todo != null)
                    todo();
                else
                    WriteLine("Action not avaliable");
            }
            while (action != ConsoleKey.D2 && action != ConsoleKey.D3);
        }

        private static void PrintGameOutcome(string s, decimal m, List<Hand> hands, House house)
        {
            Blackjack.IsRoundFinished = true;

            WriteLine("\n\nRound finished with the following outcome:");
            PrintCardsInfo(hands, house);
            Write("\n" + s);
            WriteLine($"{m}$\n");
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

        private static void PrintCardsInfo(List<Hand> hands, House house)
        {
            PrintUserCards(hands);
            PrintHouseCards(house);
        }

        private static void PrintUserCards(List<Hand> hands)
        {
            var hand = hands.ElementAt(0);

            WriteLine($"\nYour cards ({ hand.Value }" +
                      $"{ (hand.AlternativeValue != null ? "/" + hand.AlternativeValue : string.Empty) }):");
            PrintCards(hand.HandCards);
        }

        private static void PrintHouseCards(House house)
        {
            if (!Blackjack.IsRoundFinished)
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