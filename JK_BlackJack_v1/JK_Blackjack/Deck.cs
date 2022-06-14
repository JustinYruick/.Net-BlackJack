using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ServiceModel;

namespace BlackjackLibrary
{
    [ServiceContract]
    public interface IDeck
    {
        void Shuffle();
        ICard Draw();
        int NumCards { get; }

    }

    public class Deck: IDeck
    {
        /*------------------------ Member Variables ------------------------*/

        private List<Card> cards = null;    // collection of cards
        private int cardIdx;                // index of the next card to be dealt

        /*-------------------------- Constructors --------------------------*/

        public Deck()
        {
            cards = new List<Card>();
            populate();
        }

        /*------------------ Public properties and methods -----------------*/

        // Randomizes the sequence of the Cards in the cards collection
        public void Shuffle()
        {
            // Randomize the cards collection
            Random rng = new Random();
            cards = cards.OrderBy(card => rng.Next()).ToList();

            // Reset the cards index
            cardIdx = 0;
        }

        // Returns a copy of the next Card in the cards collection
        public ICard Draw()
        {
            if (cardIdx >= cards.Count)
                throw new ArgumentException("The shoe is empty.");

            return cards[cardIdx++];
        }

        public int NumCards
        {
            get
            {
                return cards.Count - cardIdx;
            }
        }


        /*------------------------- Helper methods -------------------------*/

        // Populates the cards attribute with Card objects and then shuffles it 
        private void populate()
        {
            // Clear-out all the "old' cards
            cards.Clear();
                // For each suit..
                foreach (SuitID s in Enum.GetValues(typeof(SuitID)))
                    // For each rank..
                    foreach (RankID r in Enum.GetValues(typeof(RankID)))
                            cards.Add(new Card(s, r));

            Shuffle();
        }

    } // end Shoe class
}
