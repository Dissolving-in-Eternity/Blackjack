using Blackjack.Deck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    public class Blackjack
    {
        #region Members

        // Игрок
        public List<Hand> Hands { get; private set; }

        public Hand CurHand { get; set; }

        // Крупье
        public House House { get; private set; }

        // Деньги для ставок
        public decimal Money { get; set; }

        // Колода, используемая в игре
        private Deck Cards { get; set; }

        private List<Card> FaceCards { get; set; }

        private Dictionary<Card, byte> CardValues { get; set; }

        public event GameHandler Endgame;

        public List<string> Outcomes { get; set; }

        private List<Action> OutcomeActions { get; set; }

        #endregion

        #region Init

        public Blackjack(decimal startMoney)
        {
            Cards = new Deck();
            FaceCards = new List<Card>();
            CardValues = new Dictionary<Card, byte>();
            Money = startMoney;

            AddCard(Cards);

            Hands = new List<Hand> { new Hand() };
            CurHand = Hands.ElementAt(0);
            House = new House();

            Outcomes = new List<string>();
            OutcomeActions = new List<Action>();
        }

        private void AddCard(Deck cards)
        {
            foreach (var card in cards.DeckOfCards)
            {
                switch (card.Rank)
                {
                    case Rank.Ace:
                        CardValues.Add(card, 1);
                        break;
                    case Rank.King:
                        AddFaceCard(card);
                        break;
                    case Rank.Queen:
                        AddFaceCard(card);
                        break;
                    case Rank.Valet:
                        AddFaceCard(card);
                        break;
                    case Rank.Ten:
                        CardValues.Add(card, 10);
                        break;
                    case Rank.Nine:
                        CardValues.Add(card, 9);
                        break;
                    case Rank.Eight:
                        CardValues.Add(card, 8);
                        break;
                    case Rank.Seven:
                        CardValues.Add(card, 7);
                        break;
                    case Rank.Six:
                        CardValues.Add(card, 6);
                        break;
                    case Rank.Five:
                        CardValues.Add(card, 5);
                        break;
                    case Rank.Four:
                        CardValues.Add(card, 4);
                        break;
                    case Rank.Three:
                        CardValues.Add(card, 3);
                        break;
                    case Rank.Two:
                        CardValues.Add(card, 2);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown card rank");
                }
            }
        }

        private void AddFaceCard(Card card)
        {
            FaceCards.Add(card);
            CardValues.Add(card, 10);
        }

        #endregion

        #region Deal

        public void FirstDeal()
        {
            // Перемешиваем карты
            Cards.Shuffle();

            DealUserCards(2);
            DealHouseCards(2);

            BlackjackCheckInit();

            CurHand.IsFirstDeal = false;
        }

        private void CheckUserOptions()
        {
            // Double Down
            if (Money >= CurHand.Bet && CurHand.HandCards.Count == 2)
            {
                CurHand.IsDoubleDownAvailable = true;

                // Split
                byte val1, val2;
                CardValues.TryGetValue(CurHand.HandCards[0], out val1);
                CardValues.TryGetValue(CurHand.HandCards[1], out val2);

                CurHand.IsSplitAvailable = val1 == val2;
            }
            else
            {
                CurHand.IsDoubleDownAvailable = false;
                CurHand.IsSplitAvailable = false;
            }
        }

        private void DealUserCards(int cardsToDeal)
        {
            CurHand.HandCards.AddRange(Cards.DeckOfCards.Take(cardsToDeal));
            Cards.DeckOfCards.RemoveRange(0, cardsToDeal);

            CalculateHandValues();

            CheckValues();
        }

        private void DealHouseCards(int cardsToDeal)
        {
            House.HouseCards.AddRange(Cards.DeckOfCards.Take(cardsToDeal));
            Cards.DeckOfCards.RemoveRange(0, cardsToDeal);

            // Вычисление текущего value карт
            CalculateHouseValues();

            // Обязательная проверка на Bust после каждой раздачи, кроме первой
            if(!CurHand.IsFirstDeal)
                CheckValues();
        }

        #endregion

        #region Figure Out Values

        private void CalculateHandValues()
        {
            foreach (var hand in Hands)
            {
                var altValue = hand.AlternativeValue;

                hand.Value = CalculateCurrentValue(hand.HandCards, ref altValue);

                hand.AlternativeValue = altValue;
            }
            
            CheckUserOptions();
        }

        private void CalculateHouseValues()
        {
            var altValue = House.AlternativeValue;

            House.Value = CalculateCurrentValue(House.HouseCards, ref altValue);

            House.AlternativeValue = altValue;
        }

        private byte CalculateCurrentValue(List<Card> cards, ref byte? altValue)
        {
            byte value = 0;

            // На случай 2-х тузов после 1-й раздачи
            if (CurHand.IsFirstDeal && cards.All(c => c.Rank == Rank.Ace))
            {
                value = 2;
                altValue = 12;
            }
            else if (CurHand.IsFirstDeal && BlackjackCheck(cards))
            {
                value = 21;
                altValue = null;
            }
            else
            {
                foreach (var card in cards)
                {
                    if (card.Rank == Rank.Ace)
                        altValue += 11;
                    else
                        altValue += CardValues[card];

                    value += CardValues[card];
                }

                if (altValue > 21 || altValue == value)
                    altValue = null;
            }

            return value;
        }

        #endregion

        #region Checks

        private void BlackjackCheckInit()
        {
            // Blackjack check: 
            // 1. After 1st Deal (Ok)
            // 2. After Split && 1st Deal in a new Hand (Hit, Ok)
            if (CurHand.IsFirstDeal)
            {
                var handBlackjack = BlackjackCheck(CurHand.HandCards);
                var houseBlackjack = BlackjackCheck(House.HouseCards);

                // Ничья
                if (handBlackjack && houseBlackjack)
                {
                    Outcomes.Add("Push just happened! You and House both have blackjack! (+" + CurHand.Bet + "$)");
                    OutcomeActions.Add(Push);
                    Endgame?.Invoke(this);
                }
                else if (handBlackjack)
                    BlackJackInit();
                else if (houseBlackjack)
                {
                    Outcomes.Add("House has blackjack!You lose. (- " + CurHand.Bet + "$)");
                    Endgame?.Invoke(this);
                }
            }
        }

        private bool BlackjackCheck(List<Card> cards)
        {
            // Туз + "лицевая" карта => блекджек
            if (cards.Any(c => c.Rank == Rank.Ace) && cards.Any(c => FaceCards.Contains(c)))
                return true;

            return false;
        }

        public void CheckValues(bool isFinalCheck = false)
        {
            var handValue = CurHand.Value;
            var handAltValue = CurHand.AlternativeValue;

            var houseValue = House.Value;
            var houseAltValue = House.AlternativeValue;
            
            // Проверка на Bust (после каждой раздачи) 
            BustCheck(handValue, houseValue);

            if(isFinalCheck)
                FinalCheck(handAltValue, handValue, houseAltValue, houseValue);
        }

        private void FinalCheck(byte? handAltValue, byte handValue, byte? houseAltValue, byte houseValue)
        {
            var finalHandlValue = handAltValue != null && handAltValue > handValue
                ? handAltValue
                : handValue;

            var finalHouseValue = houseAltValue != null && houseAltValue > houseValue
                ? houseAltValue
                : houseValue;

            if (finalHandlValue == finalHouseValue)
            {
                Outcomes.Add("Push just happened! You have the same value as the house. (+" + CurHand.Bet + "$)");
                OutcomeActions.Add(Push);
                Endgame?.Invoke(this);
            }
            else if (finalHandlValue > finalHouseValue)
            {
                Outcomes.Add("You have more points than the House. You won! (+" + CurHand.Bet * 2 + "$)");
                OutcomeActions.Add(Win);
                Endgame?.Invoke(this);
            }
            else if (finalHandlValue < finalHouseValue)
            {
                Outcomes.Add("House has more points than you. You lose. (-" + CurHand.Bet + "$)");
                Endgame?.Invoke(this);
            }
        }

        private void BustCheck(byte handValue, byte houseValue)
        {
            if (handValue > 21 && houseValue > 21)
            {
                Outcomes.Add("Push just happened! You and House both busted. (+" + CurHand.Bet + "$)");
                OutcomeActions.Add(Push);
                Endgame?.Invoke(this);
            }
            else if (handValue > 21)
            {
                Outcomes.Add("Bust just happened! Hand is worth more than 21. (-" + CurHand.Bet + "$)");
                Endgame?.Invoke(this);
            }
            else if (houseValue > 21)
            {
                Outcomes.Add("House has more than 21. You win! (+" + CurHand.Bet * 2 + "$)");
                OutcomeActions.Add(Win);
                Endgame?.Invoke(this);
            }
        }

        #endregion

        #region User Actions

        public void Hit()
        {
            DealUserCards(1);

            if (CurHand.HandCards.Count == 2)
                if(BlackjackCheck(CurHand.HandCards))
                    BlackJackInit();
        }

        public void Stand()
        {
            // Раздаём по 1 карте для House до тех пор, пока House Value\Alt. Value не достигнет 17
            while (House.Value < 17 ||
                House.AlternativeValue != null && House.AlternativeValue < 17)
                DealHouseCards(1);
        }

        public void DoubleDown()
        {
            if (CurHand.IsDoubleDownAvailable)
            {
                // Удваиваем ставку
                Money -= CurHand.Bet;
                CurHand.Bet *= 2;

                // Получаем ещё одну карту
                DealUserCards(1);

                // Обеспечиваем условие последней взятой карты
                if (!CurHand.IsHandOut)
                    Stand();
            }
            else
                throw new InvalidOperationException(
                    "Not all conditions satisfy Double Down availability.");
        }

        public void Split()
        {
            if (CurHand.IsSplitAvailable)
            {
                Money -= CurHand.Bet;
                
                var hand = new Hand();
                hand.HandCards.Add(CurHand.HandCards.Last());
                hand.Bet = CurHand.Bet;
                Hands.Add(hand);

                CurHand.HandCards.RemoveAt(1);

                CalculateHandValues();
            }
            else
                throw new InvalidOperationException(
                    "Not all conditions satisfy Split availability.");
        }

        #endregion

        #region Actions/Outcomes

        private void Push() => Money += CurHand.Bet;

        private void Win() => Money += CurHand.Bet * 2;

        // 3 to 2
        private void BlackJackInit()
        {
            var gain = CurHand.Bet * 2.5m;
            Money += gain;
            CurHand.Bet = 0;

            Outcomes.Add("Congratulations! You have blackjack! (+" + gain + "$)");
            Endgame?.Invoke(this);
        }

        public void ExecOutcomeActions()
        {
            foreach (var action in OutcomeActions)
                action.Invoke();
        }

        #endregion
    }
}