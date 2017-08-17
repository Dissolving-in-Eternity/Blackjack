using Blackjack.Deck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blackjack.Blackjack
{
    public class Blackjack
    {
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

        public Deck.Deck Cards { get; private set; }

        public List<Card> FaceCards { get; private set; }

        public Dictionary<Card, byte> CardValues { get; private set; }

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

        public void Deal(bool isFirstDeal, int handIndex = 0)
        {
            int cardsToDeal = isFirstDeal ? 2 : 1;

            // Перемешиваем карты
            Cards.Shuffle();

            // Раздаём по 1\2 карты на руки игроку и крупье,
            // TODO: При чём у крупье отображаем только одну из двух карт
            Hands.ElementAt(handIndex).HandCards
                .AddRange(Cards.DeckOfCards.Take(cardsToDeal));
            Cards.DeckOfCards.RemoveRange(0, cardsToDeal);

            House.HouseCards.AddRange(Cards.DeckOfCards.Take(cardsToDeal));
            Cards.DeckOfCards.RemoveRange(0, cardsToDeal);

            CalculateValues(handIndex);
        }

        private void CalculateValues(int handIndex)
        {
            Hands.ElementAt(handIndex).Value =
                CalculateCurrentValue(Hands.ElementAt(handIndex).HandCards);

            House.HouseValue = CalculateCurrentValue(House.HouseCards);
        }

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

        public void CardsCheck(int handIndex = 0)
        {
            var currentHandCards = Hands.ElementAt(handIndex).HandCards;

            // Только в случае 2-х карт проверяем на блекджек
            if (currentHandCards.Count == 2 || House.HouseCards.Count == 2)
            {
                bool handBlackjack = BlackjackCheck(currentHandCards);

                // Ничья
                if (handBlackjack && BlackjackCheck(House.HouseCards))
                    Push();
                else if (handBlackjack)
                    BlackJackInit();
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

        // 3 to 2
        private void BlackJackInit() => Money += CurrentBet * 2.5m;

        private void Push() => Money += CurrentBet;

        public void UserAction()
        {
            // TODO: Hit 

            // TODO: Stand

            // TODO: Double Down

            // TODO: Если value карт равно, возможность Split'а
        }
    }
}