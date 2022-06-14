// Coders: Justin Yurick & Kiril Trenkov
// Date: April 11th, 2021

using BlackjackLibrary;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Threading;

namespace BlackjackClient
{
    class Program
    {

        private class CBObject : IGameCallback
        {
            public void Update(string message, int nextId, bool over, List<bool> listOfWins = null)
            {
                activeClientId = nextId;
                gameOver = over;
                Console.WriteLine(message);
                if (gameOver)
                {
                    // Release all clients so they can exit out
                    waitHandle.Set();

                    Console.WriteLine($"Game over!\nYou finished with {Money} dollars!");
                    Console.WriteLine($"Press any key to quit...");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                else if (activeClientId == clientId)
                {
                    // Release this client's main thread to let this user play
                    //Console.WriteLine("It's your turn.");
                    waitHandle.Set();
                }

                if (listOfWins != null)
                {
                    listOfBools = listOfWins;
                }
            }
        }

        private static IDeck deck = null;
        public static int clientId, activeClientId = 0;
        private static CBObject cbObj = new CBObject();
        private static EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private static bool gameOver = false;
        private static List<bool> listOfBools = new List<bool>();

        public List<Card> Hand = null;
        public static double Money = 500.0;
        

        static void Main(string[] args)
        {
            List<Card> Hand = null;
            double Money = 0.0;
            bool turnOver = false;
            if (connect())
            {
                SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);

                do
                {
                    Money = 500.0;
                    Hand = new List<Card>();
                    turnOver = false;
                    Hand.Add(deck.Draw());
                    Hand.Add(deck.Draw());
                    waitHandle.WaitOne();
                    string input = "";
                    if (gameOver)
                    {
                        Console.ReadKey();
                        Environment.Exit(1);
                    }
                    else
                    {
                        if (listOfBools.Count > 0)
                        {
                            if (listOfBools[activeClientId - 1])
                            {
                                Money += 35.0;
                                Console.WriteLine("You won!");
                            }
                            else
                            {
                                Money -= 35.0;
                                Console.WriteLine("You lost!");
                            }
                        }

                        if (Money < 35.0)
                        {
                            Console.WriteLine("You don't have enough money!");
                            Console.WriteLine("Game Over!");
                            gameOver = true;
                            break;
                        }
                        Console.WriteLine($"You have {Money} dollars.", 23);
                        Console.WriteLine($"The wager is 35.00 dollars");

                        Console.WriteLine($"The dealer's hand is:\n{deck.Dealer.peek().ToString()}\n[FACE DOWN]\n");

                        Console.WriteLine("Please enter a Command \n Hit: to get a new Card \n Stand: to keep Current hand\n Hand: to view Current Hand\n Rules: to view game rules");
                        
                        try
                        {
                            input = Console.ReadLine();
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine(err.Message);
                        }
                        while (input != "Stand")
                        {
                            if (input == "Hit")
                            {
                                List<Card> myCards = deck.playerHit(activeClientId, Hand);
                                Hand = myCards;

                                printHand();

                                if (CalculateHand() > 21)
                                {
                                    Money -= 35.0;
                                    Console.WriteLine($"You lose! Current money: {Money}");
                                    break;
                                }
                                
                            }
                            else if (input == "Hand")
                            {
                                printHand();
                            }
                            else if (input == "Rules")
                            {
                                Console.WriteLine(" 1) you and the dealer start with two cards\n 2) if you go over 21 you lose\n 3) play against the dealer and have a greater score to win\n 4) bets are $35");
                            }
                            input = Console.ReadLine();
                        }

                        turnOver = true;
                        deck.EndTurn(CalculateHand());
                        waitHandle.Reset();
                    }

                } while (!gameOver);

                deck.LeaveGame();
            }
            else
            {
                Console.WriteLine("ERROR: Unable to connect to the service!");
            }


            void printHand()
            {
                Console.WriteLine("Your hand has:");
                foreach (Card card in Hand)
                {
                    Console.WriteLine(card.Rank.ToString() + " of " + card.Suit.ToString());
                }
                Console.WriteLine("Hand Value: " + CalculateHand());
            }

            int CalculateHand()
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
                    totalScore -= 10;
                }
                return totalScore;
            }
        }

        private static bool connect()
        {
            try
            {
                DuplexChannelFactory<IDeck> channel = new DuplexChannelFactory<IDeck>(cbObj, "ClientCallback");
                deck = channel.CreateChannel();

                Console.WriteLine("~ Welcome to Justin & Kiril's Blackjack Game! ~\n");

                // Register for the callbacks (tells the Shoe object to include this instance of 
                // the client in future callback events (i.e. updates)
                clientId = deck.JoinGame();

                if (clientId == 1)
                    // Only client connected so far so release this client to do the first "count"
                    // (necessary because a callback will only happen when a "count" is performed 
                    // and a callback is the mechanism used to release a client)
                    cbObj.Update("", 1, false);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                case CtrlTypes.CTRL_BREAK_EVENT:
                    break;
                case CtrlTypes.CTRL_CLOSE_EVENT:
                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    deck?.LeaveGame();
                    break;
            }
            return true;
        }

        #region unmanaged

        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.

        [DllImport("Kernel32")]

        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        #endregion
    }
}