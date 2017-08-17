using Blackjack.Deck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blackjack.Blackjack
{
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
            get { return _currentBet; }
            set
            {
                if (value > Money)
                    throw new ArgumentException("Not enough money", nameof(value));

                _currentBet = value;

                Money -= value;
            }
        }

        // Колода, используемая в игре
        public Deck.Deck Cards { get; private set; }

        public List<Card> FaceCards { get; private set; }

        public Dictionary<Card, byte> CardValues { get; private set; }

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

        public void FirstDeal(int handIndex = 0)
        {
            // Перемешиваем карты
            Cards.Shuffle();

            DealUserCards(2, handIndex);
            DealHouseCards(2);
        }

        private void DealHouseCards(int cardsToDeal)
        {
            House.HouseCards.AddRange(Cards.DeckOfCards.Take(cardsToDeal));
            Cards.DeckOfCards.RemoveRange(0, cardsToDeal);

            CalculateHouseValues();
        }

        private void DealUserCards(int cardsToDeal, int handIndex = 0)
        {
            Hands.ElementAt(handIndex).HandCards
                .AddRange(Cards.DeckOfCards.Take(cardsToDeal));
            Cards.DeckOfCards.RemoveRange(0, cardsToDeal);

            CalculateHandValues(handIndex);
        }

        #endregion

        #region Figure Out Values

        private void CalculateHandValues(int handIndex)
        {
            Hands.ElementAt(handIndex).Value =
                CalculateCurrentValue(Hands.ElementAt(handIndex).HandCards);
        }

        private void CalculateHouseValues() =>
            House.HouseValue = CalculateCurrentValue(House.HouseCards);

        private byte CalculateCurrentValue(List<Card> cards)
        {
            byte value = 0;

            foreach (var card in cards)
            {
                if (CardValues.ContainsKey(card))
                    value += CardValues[card];
            }

            return value;
        }

        #endregion

        #region Checks

        public void CheckCards(int handIndex = 0)
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
                    Push();
                else if (handBlackjack)
                    BlackJackInit();
                else if (houseBlackjack)
                    Lose();
            }

            // TODO: Если отображаемая карта у крупье - туз, предлагаем страховку
        }

        private bool BlackjackCheck(List<Card> cards)
        {
            // Туз + "лицевая" карта => блекджек
            if (cards.Any(c => c.Rank == Rank.Ace) && cards.Any(c => FaceCards.Contains(c)))
                return true;

            return false;
        }

        public void CheckValues(int handIndex = 0)
        {
            var currenHandValue = Hands.ElementAt(handIndex).Value;
            var currenHouseValue = House.HouseValue;

            if (currenHandValue > 21 && currenHouseValue > 21)
                Push();
            else if (currenHandValue > 21)
                Bust();
            else if (currenHouseValue > 21)
                Win();
            else if(currenHandValue == currenHouseValue)
                Push();
            else if(currenHandValue > currenHouseValue)
                Win();
            else if(currenHouseValue > currenHandValue)
                Lose();
        }

        #endregion

        #region User Actions

        public void Hit()
        {
            DealUserCards(1);

            if(Hands.ElementAt(0).Value > 21)
                Bust();
        }

        public void Stand()
        {
            // Раздаём по 1 карте для House до тех пор, пока House Value не достигнет 17
            while (House.HouseValue < 17)
                DealHouseCards(1);
        }

        // TODO: Double Down

        // TODO: Split

        #endregion

        #region Actions/Outcomes

        private void Bust() => CurrentBet = 0;

        private void Push() => Money += CurrentBet;

        private void Win()
        {
            Money += CurrentBet * 2;
            CurrentBet = 0;
        }

        private void Lose() => CurrentBet = 0;

        // 3 to 2
        private void BlackJackInit() => Money += CurrentBet * 2.5m;

        #endregion
    }
}