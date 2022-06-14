// Coders: Justin Yurick & Kiril Trenkov
// Date: April 11th, 2021

using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace BlackjackLibrary
{
    // Converted IDealer to a WCF service contract
    [ServiceContract]
    public interface IDealer
    {
        [OperationContract]
        int CalculateHand();
        [OperationContract]
        bool PlayAgainst(int score);
        [OperationContract]
        void drawCard(IDeck deck);

        [OperationContract]
        void printHand();
        [OperationContract]
        Card peek();
    }

    // The class that implements the service
    // ServiceBehavior is used here to select the desired instancing behaviour
    // of the Dealer class
    [DataContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class Dealer : IDealer
    {
        [DataMember]
        public List<Card> Hand = null;

        public Dealer(IDeck deck)
        {
            Hand = new List<Card>();
            Hand.Add(deck.Draw());
            Hand.Add(deck.Draw());
        }

        // Get the score of the dealer
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

        // Compare player score to dealer's score
        public bool PlayAgainst(int score)
        {
            if ((score <= 21 && CalculateHand() > 21) || (score <= 21 && score > this.CalculateHand()))
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

        public void printHand()
        {
            Console.WriteLine("The Dealer has:");
            foreach (Card card in Hand)
            {
                Console.WriteLine(card.Rank.ToString() + " of " + card.Suit.ToString());
            }
            Console.WriteLine("Dealers Value: " + CalculateHand());
        }

        public string handToString()
        {
            string returnVal = "";
            foreach (Card card in Hand)
            {
                returnVal += card.Rank.ToString() + " of " + card.Suit.ToString() + "\n";
            }
            return returnVal;
        }

        public Card peek()
        {
            return Hand[0];
        }
    }
}
