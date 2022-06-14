using System;
using System.Collections.Generic;
using System.Text;

namespace BlackjackLibrary
{
    public interface IDealer
    {
        int CalculateHand();
        bool PlayAgainst(int score);
        void drawCard(IDeck deck);
    }
    public class Dealer : IDealer
    {
        private List<ICard> Hand = null;

        public Dealer(IDeck deck)
        {
            
            Hand = new List<ICard>();
            Hand.Add(deck.Draw());
            Hand.Add(deck.Draw());
        }


        public int CalculateHand()
        {
            int totalScore = 0;
            bool acePresent = false;
            foreach(Card card in Hand)
            {
                if(card.GetValue() == 11)
                {
                    acePresent = true;
                }
                totalScore += card.GetValue();
            }
            if(acePresent == true && totalScore > 21)
            {
                totalScore =- 10;
            }
            return totalScore;
        }

        public bool PlayAgainst(int score)
        {
            if(score > this.CalculateHand())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void drawCard(IDeck deck)
        {
            Hand.Add(deck.Draw());
        }
    }
}
