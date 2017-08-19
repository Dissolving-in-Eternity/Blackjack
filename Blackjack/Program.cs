using Blackjack.Deck;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Console;

namespace Blackjack
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var bj = new Blackjack.Blackjack(5000);

            SubscribeToEvents(bj);

            WriteLine("Welcome to the Blackjack by Pride!\n");
            WriteLine($"Your current amount of money is { bj.Money }.\n");

            PlaceABet(bj);

            bj.FirstDeal();

            //PrintCardsInfo(bj);

            bj.BlackjackCheckInit();

            PerformUserAction(bj);

            bj.CheckValues(true);

            ReadLine();
        }

        private static void PerformUserAction(Blackjack.Blackjack bj)
        {
            ConsoleKeyInfo action;

            do
            {
                PrintCardsInfo(bj);

                WriteLine("\n1. Hit\n2. Stand\n");
                Write("Your action: ");
                action = ReadKey();

                if (action.Key != ConsoleKey.D1 && action.Key != ConsoleKey.D2)
                {
                    WriteLine("\nWrong character.");
                    continue;
                }

                if (action.Key == ConsoleKey.D1)
                    bj.Hit();
                else
                    bj.Stand();

            } while (action.Key != ConsoleKey.D2);

            PrintCardsInfo(bj);
        }

        private static void PrintGameOutcome(string s, decimal m)
        {
            Write("\n" + s);
            WriteLine($"{m}$\n");
        }

        private static void PlaceABet(Blackjack.Blackjack bj)
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

        private static void PrintCardsInfo(Blackjack.Blackjack bj)
        {
            PrintUserCards(bj);

            PrintHouseCards(bj);
        }

        private static void PrintUserCards(Blackjack.Blackjack bj)
        {
            var hands = bj.Hands.ElementAt(0);

            WriteLine($"\n\nYour cards ({hands.Value}" +
                      $"{(hands.AlternativeValue != null ? "/" + hands.AlternativeValue : string.Empty)}):");
            PrintCards(hands.HandCards);
        }

        private static void PrintHouseCards(Blackjack.Blackjack bj)
        {
            WriteLine($"\nHouse cards ({bj.House.Value}" +
                      $"{(bj.House.AlternativeValue != null ? "/" + bj.House.AlternativeValue : string.Empty)}):");

            PrintCards(bj.House.HouseCards);
        }

        private static void PrintCards(List<Card> cards)
        {
            foreach (var card in cards)
                WriteLine(card);
        }

        private static void SubscribeToEvents(Blackjack.Blackjack bj)
        {
            bj.BustEvent += PrintGameOutcome;
            bj.PushEvent += PrintGameOutcome;
            bj.WinEvent += PrintGameOutcome;
            bj.LoseEvent += PrintGameOutcome;
            bj.BlackjackEvent += PrintGameOutcome;
        }
    }
}