using System;

namespace BlackjackLibrary
{
    public enum SuitID { Clubs, Diamonds, Hearts, Spades };
    public enum RankID { Ace, King, Queen, Jack, Ten, Nine, Eight, Seven, Six, Five, Four, Three, Two };

public interface ICard
    {
        SuitID Suit { get; }
        RankID Rank { get; }
        string ToString();

        int GetValue();
    }

    // The class definition is now invisible from outside the CardsLibrary assembly
    public class Card : ICard
    {
        /*-------------------------- Constructors --------------------------*/

        public Card(SuitID s, RankID r)
        {
            Suit = s;
            Rank = r;
        }

        /*------------------ Public properties and methods -----------------*/

        public SuitID Suit { get; private set; }

        public RankID Rank { get; private set; }

        public override string ToString()
        {
            return Rank.ToString() + " of " + Suit.ToString();
        }

        public int GetValue()
        {
            switch (this.Rank.ToString())
            {
                case "Two":
                    return 2;
                case "Three":
                    return 3;
                case "Four":
                    return 4;
                case "Five":
                    return 5;
                case "Six":
                    return 6;
                case "Seven":
                    return 7;
                case "Eight":
                    return 8;
                case "Nine":
                    return 9;
                case "Ten":
                    return 10;
                case "Jack":
                    return 10;
                case "King":
                    return 10;
                case "Queen":
                    return 10;
                case "Ace":
                    return 11;
                default:
                    return 0;
            }

        }

    } // end Card class
}
