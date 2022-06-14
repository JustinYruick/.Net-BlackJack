using System;
using System.Collections.Generic;
using System.Text;

namespace BlackjackLibrary
{
    public interface IPlayer
    {
        int CalculateHand();
        void drawCard(IDeck deck);
    }
    public class Player : IPlayer
    {
        private List<ICard> Hand = null;
        private double Money = 0.0;

        public Player(IDeck deck)
        {
            Money = 500.0; 
            Hand = new List<ICard>();
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
    }
}
