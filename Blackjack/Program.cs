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

            WriteLine("Welcome to the Blackjack by Pride!\n");
            WriteLine("Blackjack pays 3 to 2.");
            WriteLine("Dealer must draw to 16 and stand on 17.");

            do
            {
                if (bj.Hands.All(h => h.IsHandOut))
                {
                    var moneyFromTheLastRound = bj.Money;
                    bj = new Blackjack(moneyFromTheLastRound);

                    bj.CurHand.IsHandOut = false;
                }

                bj.Endgame += PrintGameOutcome;

                WriteLine($"\nYour current amount of money is { bj.Money }.\n");

                PlaceABet(bj);

                bj.FirstDeal();

                PerformUserAction(bj);

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
                    { ConsoleKey.D2, bj.Stand }
                };

            for (var i = 0; i < bj.Hands.Count; i++)
            {
                bj.CurHand = bj.Hands[i];

                ConsoleKey action;

                do
                {
                    if (bj.CurHand.IsHandOut)
                        break;

                    if (bj.Hands.Count > 1)
                        WriteLine($"\nHand {i + 1}:");

                    PrintUserCards(bj.CurHand);
                    PrintHouseCards(bj.House);

                    CheckUserOptions(bj, actions);

                    if (bj.Hands.Count > 1)
                        WriteLine($"Avaliable actions for hand {i + 1}:");

                    PrintActions(actions);

                    Write("\nYour action: ");
                    action = ReadKey().Key;
                    WriteLine();

                    if (!actions.ContainsKey(action))
                    {
                        WriteLine("\nWrong character.");
                        continue;
                    }

                    actions.TryGetValue(action, out Action todo);

                    if (todo != null)
                        todo();
                    else
                        WriteLine("Action not avaliable");

                } while (actions[action].Method.Name != "Stand" && actions[action].Method.Name != "DoubleDown");

                if (!bj.CurHand.IsHandOut)
                    bj.CheckValues(true);
            }
        }

        private static void CheckUserOptions(Blackjack bj, Dictionary<ConsoleKey, Action> actions)
        {
            if (!bj.CurHand.IsDoubleDownAvailable && actions.ContainsKey(ConsoleKey.D3))
                actions.Remove(ConsoleKey.D3);

            if (bj.CurHand.IsDoubleDownAvailable && !actions.ContainsKey(ConsoleKey.D3))
                actions.Add(ConsoleKey.D3, bj.DoubleDown);

            if (!bj.CurHand.IsSplitAvailable && actions.ContainsKey(ConsoleKey.D4))
                actions.Remove(ConsoleKey.D4);

            else if (bj.CurHand.IsSplitAvailable && !actions.ContainsKey(ConsoleKey.D4))
                actions.Add(ConsoleKey.D4, bj.Split);
        }

        private static void PrintActions(Dictionary<ConsoleKey, Action> actions)
        {
            for (var j = 0; j < actions.Count; j++)
                WriteLine($"{actions.Keys.ElementAt(j)}. {actions.Values.ElementAt(j).Method.Name}");
        }

        private static void PrintGameOutcome(Blackjack bj)
        {
            bj.CurHand.IsHandOut = true;

            if(bj.Hands.Count > 1)
                WriteLine("\nRound for the current hand is over.");

            // If round is over
            if (bj.Hands.All(h => h.IsHandOut))
            {
                bj.ExecOutcomeActions();

                WriteLine("\nRound finished!");

                if(bj.Hands.Count > 1)
                    for (int i = 0; i < bj.Hands.Count; i++)
                    {
                        WriteLine($"\nHand { i + 1 }:");
                        PrintUserCards(bj.Hands[i]);
                    }
                else
                    PrintUserCards(bj.CurHand);

                PrintHouseCards(bj.House, true);

                for (var i = 0; i < bj.Outcomes.Count; i++)
                {
                    if(bj.Hands.Count > 1)
                        WriteLine($"Hand { i + 1 }:");
                    WriteLine($"{ bj.Outcomes[i] }\n");
                }
            }
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

                    bj.CurHand.Bet = bet;
                    bj.Money -= bj.CurHand.Bet;

                    break;
                }
                catch (FormatException)
                {
                    WriteLine("You must enter a whole number.");
                }
            }
        }

        private static void PrintUserCards(Hand hand)
        {
            WriteLine($"\nYour cards ({ hand.Value }" +
                        $"{ (hand.AlternativeValue != null ? "/" + hand.AlternativeValue : string.Empty) }):");
            PrintCards(hand.HandCards);
        }

        private static void PrintHouseCards(House house, bool printAllCards = false)
        {
            if(!printAllCards)
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

            WriteLine();
        }

        private static void PrintCards(List<Card> cards)
        {
            foreach (var card in cards)
                WriteLine(" - " + card);
        }
    }
}