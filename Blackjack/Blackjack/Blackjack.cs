using Blackjack.Deck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blackjack.Blackjack
{
    public delegate void BustHandler(string s, decimal m);
    public delegate void PushHandler(string s, decimal m);
    public delegate void WinHandler(string s, decimal m);
    public delegate void LoseHandler(string s, decimal m);
    public delegate void BlackjackHandler(string s, decimal m);

    public class Blackjack
    {
        #region Members

        // Игрок
        public List<Hand> Hands { get; set; }

        // Крупье
        public House House { get; set; }

        // Деньги для ставок
        public decimal Money { get; private set; }

        // Ставка в текущем раунде
        private decimal _currentBet;

        public decimal CurrentBet
        {
            get => _currentBet;
            set
            {
                if (value > Money)
                    throw new ArgumentException("Not enough money.");

                if(value < 0)
                    throw new ArgumentOutOfRangeException(null, "Bet can't be less than 0");

                _currentBet = value;

                Money -= value;
            }
        }

        // Колода, используемая в игре
        public Deck.Deck Cards { get; private set; }

        public List<Card> FaceCards { get; private set; }

        public Dictionary<Card, byte> CardValues { get; private set; }

        public event BustHandler BustEvent;
        public event PushHandler PushEvent;
        public event WinHandler WinEvent;
        public event LoseHandler LoseEvent;
        public event BlackjackHandler BlackjackEvent;

        #endregion

        #region Init

        public Blackjack(decimal startMoney)
        {
            Cards = new Deck.Deck();
            FaceCards = new List<Card>();
            CardValues = new Dictionary<Card, byte>();
            Money = startMoney;

            AddCard(Cards);

            Hands = new List<Hand> {new Hand()};
            House = House.GetHouse;
        }

        private void AddCard(Deck.Deck cards)
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
        }

        public void DealHouseCards(int cardsToDeal)
        {
            House.HouseCards.AddRange(Cards.DeckOfCards.Take(cardsToDeal));
            Cards.DeckOfCards.RemoveRange(0, cardsToDeal);

            // Вычисление текущего value карт
            CalculateHouseValues();

            // Обязательная проверка на Bust после каждой раздачи, кроме первой
            if(House.HouseCards.Count > 2)
                CheckValues();
        }

        public void DealUserCards(int cardsToDeal, int handIndex = 0)
        {
            Hands.ElementAt(handIndex).HandCards
                .AddRange(Cards.DeckOfCards.Take(cardsToDeal));
            Cards.DeckOfCards.RemoveRange(0, cardsToDeal);

            CalculateHandValues(handIndex);

            if(Hands.ElementAt(handIndex).HandCards.Count > 2)
                CheckValues();
        }

        #endregion

        #region Figure Out Values

        private void CalculateHandValues(int handIndex)
        {
            var hand = Hands.ElementAt(handIndex);
            var altValue = hand.AlternativeValue;

            hand.Value = CalculateCurrentValue(hand.HandCards, ref altValue);

            hand.AlternativeValue = altValue;
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
            bool isFirstDeal = cards.Count == 2;

            // На случай 2-х тузов после 1-й раздачи
            if (isFirstDeal && cards.All(c => c.Rank == Rank.Ace))
            {
                value = 2;
                altValue = 12;
            }
            else if (isFirstDeal && BlackjackCheck(cards))
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

        public void BlackjackCheckInit(int handIndex = 0)
        {
            var currentHandCards = Hands.ElementAt(handIndex).HandCards;

            // Blackjack check: 
            // 1. After 1st Deal (Ok)
            // TODO: 2. After Split && 1st Deal in a new Hand
            if (currentHandCards.Count == 2 && House.HouseCards.Count == 2)
            {
                bool handBlackjack = BlackjackCheck(currentHandCards);
                bool houseBlackjack = BlackjackCheck(House.HouseCards);

                // Ничья
                if (handBlackjack && houseBlackjack)
                {
                    PushEvent?.Invoke("Push just happened! You and House both have blackjack! \n\n+", CurrentBet);
                    Push();
                }
                else if (handBlackjack)
                    BlackJackInit();
                else if (houseBlackjack)
                {
                    LoseEvent?.Invoke("House has blackjack! You lose. \n\n-", CurrentBet);
                    Lose();
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

        public void CheckValues(bool isFinalCheck = false, int handIndex = 0)
        {
            var handValue = Hands.ElementAt(handIndex).Value;
            var handAltValue = Hands.ElementAt(handIndex).AlternativeValue;

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
                PushEvent?.Invoke("Push just happened! You have the same value as the house. \n\n+", CurrentBet);
                Push();
            }
            else if (finalHandlValue > finalHouseValue)
            {
                WinEvent?.Invoke("You have more points than the House. You won! \n\n+", CurrentBet * 2);
                Win();
            }
            else if (finalHandlValue < finalHouseValue)
            {
                LoseEvent?.Invoke("House has more points than you. You lose. \n\n-", CurrentBet);
                Lose();
            }
        }

        private void BustCheck(byte handValue, byte houseValue)
        {
            if (handValue > 21 && houseValue > 21)
            {
                PushEvent?.Invoke("Push just happened! You and House both busted. \n\n+", CurrentBet);
                Push();
            }
            else if (handValue > 21)
            {
                BustEvent?.Invoke("Bust just happened! Hand is worth more than 21. \n\n-", CurrentBet);
                Bust();
            }
            else if (houseValue > 21)
            {
                WinEvent?.Invoke("House has more than 21. You win! \n\n+", CurrentBet * 2);
                Win();
            }
        }

        #endregion

        #region User Actions

        public void Hit()
        {
            DealUserCards(1);
        }

        public void Stand()
        {
            // Раздаём по 1 карте для House до тех пор, пока House Value\Alt. Value не достигнет 17
            while (House.Value < 17 ||
                (House.AlternativeValue != null && House.AlternativeValue < 17))
                DealHouseCards(1);
        }

        // TODO: Test and Improve
        public void DoubleDown(int handIndex)
        {
            // Если количество карт на руках - 2
            if (Hands.ElementAt(handIndex).HandCards.Count == 2)
            {
                // Хватает ли денег, чтобы удвоить ставку
                if (Money >= CurrentBet * 2)
                {
                    // Удваиваем ставку
                    CurrentBet *= 2;
                    Money -= CurrentBet * 2;

                    // Получаем ещё одну карту (TODO: и только одну до конца раунда)
                    DealUserCards(1);
                }
            }
        }

        // TODO: Split

        #endregion

        #region Actions/Outcomes

        private void Bust() => CurrentBet = 0;

        private void Push()
        {
            Money += CurrentBet;
            CurrentBet = 0;
        }

        private void Win()
        {
            Money += CurrentBet * 2;
            CurrentBet = 0;
        }

        private void Lose() => CurrentBet = 0;

        // 3 to 2
        private void BlackJackInit()
        {
            var gain = CurrentBet * 2.5m;
            Money += gain;
            CurrentBet = 0;

            BlackjackEvent?.Invoke("Congratulations! You have blackjack! \n\n+", gain);
        }

        #endregion
    }
}