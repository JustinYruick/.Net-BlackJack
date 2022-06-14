
using BlackjackLibrary;
using System;

namespace JK_BlackjackClient
{
    class Program
    {
        static void Main(string[] args)
        {

            // Report the # of decks and cards
            IDeck deck = new Deck();
            IDealer dealer = new Dealer(deck);
            IPlayer player1 = new Player(deck);

            Console.WriteLine($"The dealers hand is worth: {dealer.CalculateHand()}");

            Console.WriteLine($"The player 1 hand is worth: {player1.CalculateHand()}");

           bool won = dealer.PlayAgainst(player1.CalculateHand());
            if(won == true)
            {
                Console.WriteLine("the Player wins");
            }
            else
            {
                Console.WriteLine("the player lost");
            }
            //try
            //{
            //    // Draw cards
            //    while (deck.NumCards > 0)
            //    {
            //        ICard c = deck.Draw();
            //        Console.WriteLine(c.ToString() + " " +  c.GetValue());
            //    }
            //}
            //catch (ArgumentException ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }
    }
}
