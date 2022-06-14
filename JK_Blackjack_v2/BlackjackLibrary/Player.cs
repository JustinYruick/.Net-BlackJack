// Coders: Justin Yurick & Kiril Trenkov
// Date: April 11th, 2021

using System;
using System.Collections.Generic;

namespace BlackjackLibrary
{
    public interface IPlayer
    {
        int CalculateHand();
        void drawCard(IDeck deck);

        void printHand();
    }
    public class Player : IPlayer
    {
        private List<Card> Hand = null;
        private double Money = 0.0;

        public Player(IDeck deck)
        {
            Money = 500.0; 
            Hand = new List<Card>();
            Hand.Add(deck.Draw());
            Hand.Add(deck.Draw());
        }

        public void setMoney(double amount)
        {
            this.Money = amount;
        }

        public double getMoney()
        {
            return this.Money;
        }

        public int CalculateHand()
        {
            int totalScore = 0;
            bool acePresent = false;
            foreach (Card card in Hand)
            {
                if (card.GetValue() == 11)
                {
                    acePresent = true;
                }
                totalScore += card.GetValue();
            }
            if (acePresent == true && totalScore > 21)
            {
                totalScore = -10;
            }
            return totalScore;
        }

        public void drawCard(IDeck deck)
        {
            Hand.Add(deck.Draw());
        }

        public void printHand()
        {
            Console.WriteLine("Your hand has:");
            foreach(Card card in Hand)
            {
                Console.WriteLine(card.Rank.ToString() + " of " + card.Suit.ToString());
            }
            Console.WriteLine("Hand Value: " + CalculateHand());
        }

    }
}
