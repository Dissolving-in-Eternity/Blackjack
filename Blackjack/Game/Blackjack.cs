using System;
using System.Collections.Generic;
using System.Linq;
using Blackjack.Cards;

namespace Blackjack.Game
{
    public class Blackjack
    {
        #region Members

        // Player
        public List<Hand> Hands { get; private set; }

        public Hand CurHand { get; set; }

        public House House { get; private set; }

        // Money to bet
        public decimal Money { get; set; }

        private Deck _cards { get; set; }

        private List<Card> _faceCards { get; set; }

        private Dictionary<Card, byte> _cardValues { get; set; }

        public event RoundHandler Endgame;

        public List<string> Outcomes { get; set; }

        private List<Action> _outcomeActions { get; set; }

        // Ace may count as 1 or 11
        private const byte AceValue = 1;
        private const byte AceAltValue = 11;

        private const byte FaceCardValue = 10;

        private const byte BlackjackValue = 21;

        private byte _cardsToDeal;

        #endregion

        #region Init

        public Blackjack(decimal startMoney)
        {
            _cards = new Deck();
            _faceCards = new List<Card>();
            _cardValues = new Dictionary<Card, byte>();
            Money = startMoney;

            AddCard(_cards);

            Hands = new List<Hand> { new Hand() };
            CurHand = Hands.ElementAt(0);
            House = new House();

            Outcomes = new List<string>();
            _outcomeActions = new List<Action>();

            _cardsToDeal = 2;
        }

        private void AddCard(Deck cards)
        {
            foreach (var card in cards.DeckOfCards)
            {
                switch (card.Rank)
                {
                    case Rank.Ace:
                        _cardValues.Add(card, 1);
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
                        _cardValues.Add(card, 10);
                        break;
                    case Rank.Nine:
                        _cardValues.Add(card, 9);
                        break;
                    case Rank.Eight:
                        _cardValues.Add(card, 8);
                        break;
                    case Rank.Seven:
                        _cardValues.Add(card, 7);
                        break;
                    case Rank.Six:
                        _cardValues.Add(card, 6);
                        break;
                    case Rank.Five:
                        _cardValues.Add(card, 5);
                        break;
                    case Rank.Four:
                        _cardValues.Add(card, 4);
                        break;
                    case Rank.Three:
                        _cardValues.Add(card, 3);
                        break;
                    case Rank.Two:
                        _cardValues.Add(card, 2);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown card rank");
                }
            }
        }

        private void AddFaceCard(Card card)
        {
            _faceCards.Add(card);
            _cardValues.Add(card, FaceCardValue);
        }

        #endregion

        #region Deal

        public void FirstDeal()
        {
            _cards.Shuffle();

            DealUserCards(_cardsToDeal);
            DealHouseCards(_cardsToDeal);

            BlackjackCheckInit();

            CurHand.IsFirstDeal = false;
            _cardsToDeal = 1;
        }

        private void CheckUserOptions()
        {
            // Double Down availability
            if (Money < CurHand.Bet || CurHand.HandCards.Count != 2)
            {
                CurHand.IsDoubleDownAvailable = false;
                CurHand.IsSplitAvailable = false;

                return;
            }

            CurHand.IsDoubleDownAvailable = true;

            // Split availability
            _cardValues.TryGetValue(CurHand.HandCards[0], out var firstCardOnHandValue);
            _cardValues.TryGetValue(CurHand.HandCards[1], out var secondCardOnHandValue);

            CurHand.IsSplitAvailable = firstCardOnHandValue == secondCardOnHandValue;
        }

        private void DealUserCards(int cardsToDeal)
        {
            CurHand.HandCards.AddRange(_cards.DeckOfCards.Take(cardsToDeal));
            _cards.DeckOfCards.RemoveRange(0, cardsToDeal);

            CalculateHandValues();

            CheckValues();
        }

        private void DealHouseCards(int cardsToDeal)
        {
            House.HouseCards.AddRange(_cards.DeckOfCards.Take(cardsToDeal));
            _cards.DeckOfCards.RemoveRange(0, cardsToDeal);

            CalculateHouseValues();

            // Mandatory Bust check after every deal except the first
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
            // In case of 2 aces after the first deal
            if (CurHand.IsFirstDeal && cards.All(c => c.Rank == Rank.Ace))
            {
                altValue = AceValue + AceAltValue;

                return AceValue + AceValue;
            }

            if (CurHand.IsFirstDeal && BlackjackCheck(cards))
            {
                altValue = null;

                return BlackjackValue;
            }

            byte value = 0;

            foreach (var card in cards)
            {
                if (card.Rank == Rank.Ace)
                    altValue += AceAltValue;
                else
                    altValue += _cardValues[card];

                value += _cardValues[card];
            }

            if (altValue > BlackjackValue || altValue == value)
                altValue = null;
            
            return value;
        }

        #endregion

        #region Checks

        private void BlackjackCheckInit()
        {
            if (!CurHand.IsFirstDeal)
                return;

            var handBlackjack = BlackjackCheck(CurHand.HandCards);
            var houseBlackjack = BlackjackCheck(House.HouseCards);

            if (handBlackjack && houseBlackjack)
            {
                Outcomes.Add("Push just happened! You and House both have blackjack! (+" + CurHand.Bet + "$)");
                _outcomeActions.Add(Push);
                Endgame?.Invoke(this);

                return;
            }

            if (handBlackjack)
            {
                BlackJackInit();

                return;
            }

            if (houseBlackjack)
            {
                Outcomes.Add("House has blackjack!You lose. (- " + CurHand.Bet + "$)");
                Endgame?.Invoke(this);
            }
        }

        private bool BlackjackCheck(List<Card> cards)
        {
            // Ace + face card == blackjack
            return cards.Any(c => c.Rank == Rank.Ace) && 
                cards.Any(c => _faceCards.Contains(c));
        }

        public void CheckValues(bool isFinalCheck = false)
        {
            var handValue = CurHand.Value;
            var handAltValue = CurHand.AlternativeValue;

            var houseValue = House.Value;
            var houseAltValue = House.AlternativeValue;
            
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
                _outcomeActions.Add(Push);
                Endgame?.Invoke(this);

                return;
            }

            if (finalHandlValue > finalHouseValue)
            {
                Outcomes.Add("You have more points than the House. You won! (+" + CurHand.Bet * 2 + "$)");
                _outcomeActions.Add(Win);
                Endgame?.Invoke(this);

                return;
            }

            if (finalHandlValue < finalHouseValue)
            {
                Outcomes.Add("House has more points than you. You lose. (-" + CurHand.Bet + "$)");
                Endgame?.Invoke(this);
            }
        }

        private void BustCheck(byte handValue, byte houseValue)
        {
            if (handValue > BlackjackValue && houseValue > BlackjackValue)
            {
                Outcomes.Add("Push just happened! You and House both busted. (+" + CurHand.Bet + "$)");
                _outcomeActions.Add(Push);
                Endgame?.Invoke(this);

                return;
            }

            if (handValue > BlackjackValue)
            {
                Outcomes.Add("Bust just happened! Hand is worth more than 21. (-" + CurHand.Bet + "$)");
                Endgame?.Invoke(this);

                return;
            }

            if (houseValue > BlackjackValue)
            {
                Outcomes.Add("House has more than 21. You win! (+" + CurHand.Bet * 2 + "$)");
                _outcomeActions.Add(Win);
                Endgame?.Invoke(this);
            }
        }

        #endregion

        #region User Actions

        public void Hit()
        {
            DealUserCards(_cardsToDeal);

            if(CurHand.HandCards.Count == 2 && BlackjackCheck(CurHand.HandCards))
                BlackJackInit();
        }

        public void Stand()
        {
            while (House.Value < House.DrawTo ||
                House.AlternativeValue != null && House.AlternativeValue < House.DrawTo)
                DealHouseCards(_cardsToDeal);
        }

        public void DoubleDown()
        {
            if (!CurHand.IsDoubleDownAvailable)
                throw new InvalidOperationException(
                    "Not all conditions satisfy Double Down availability.");

            // Double a bet
            Money -= CurHand.Bet;
            CurHand.Bet *= 2;

            // Get another card
            DealUserCards(_cardsToDeal);

            // Make sure that card user just got is the last one until the end of the round
            if (!CurHand.IsHandOut)
                Stand();
        }

        public void Split()
        {
            if (!CurHand.IsSplitAvailable)
                throw new InvalidOperationException(
                    "Not all conditions satisfy Split availability.");

            // Substruct money required for a new hand
            Money -= CurHand.Bet;
            
            // Create a new hand and add money to it
            var hand = new Hand();
            hand.HandCards.Add(CurHand.HandCards.Last());
            hand.Bet = CurHand.Bet;
            Hands.Add(hand);

            // Remove the last (index 1) card from current hand
            CurHand.HandCards.RemoveAt(1);

            CalculateHandValues();
        }

        #endregion

        #region Actions/Outcomes

        private void Push() => Money += CurHand.Bet;

        private void Win() => Money += CurHand.Bet * 2;

        private void BlackJackInit()
        {
            // 3 to 2
            var gain = CurHand.Bet * 2.5m;
            Money += gain;
            CurHand.Bet = 0;

            Outcomes.Add("Congratulations! You have blackjack! (+" + gain + "$)");
            Endgame?.Invoke(this);
        }

        public void ExecOutcomeActions()
        {
            foreach (var action in _outcomeActions)
                action.Invoke();
        }

        #endregion
    }
}