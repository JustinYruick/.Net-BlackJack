// Coders: Justin Yurick & Kiril Trenkov
// Date: April 11th, 2021

using System;
using System.Collections.Generic;
using System.Linq;

using System.ServiceModel;

namespace BlackjackLibrary
{
    public interface IGameCallback
    {
        [OperationContract(IsOneWay = true)]
        void Update(string message, int nextClient, bool gameOver, List<bool> listOfWins = null);
    }

    // Converted IDeck to a WCF service contract
    [ServiceContract(CallbackContract = typeof(IGameCallback))]
    public interface IDeck
    {
        [OperationContract]
        void Shuffle();
        [OperationContract]
        Card Draw();
        int NumCards { [OperationContract] get; }

        Dealer Dealer { [OperationContract] get; }

        [OperationContract]
        int JoinGame();
        [OperationContract(IsOneWay = true)]
        void LeaveGame();
        [OperationContract(IsOneWay = true)]
        void EndTurn(int score);
        [OperationContract(IsOneWay = true)]
        void dealerHit();
        [OperationContract]
        List<Card> dealerPrint();
        [OperationContract]
        List<Card> playerHit(int playerindex, List<Card> currentHand);

    }

    // The class that implements the service
    // ServiceBehavior is used here to select the desired instancing behaviour
    // of the Deck class
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Deck : IDeck
    {
        /*------------------------ Member Variables ------------------------*/

        private List<Card> cards = null;    // collection of cards
        private int cardIdx;                // index of the next card to be dealt

        public Dealer Dealer = null;

        private Dictionary<int, IGameCallback> callbacks = null;    // Stores id and callback for each client
        private int nextClientId;                               // Unique Id to be assigned of next client that "Joins"
        private int clientIndex;                                // Index of client that "counts" next
        private bool gameOver = false;
        private int roundOverCounter;
        private int roundCounter;

        private List<int> listOfScores = null;
        int dealerScore;
        List<bool> listOfWins = null;

        /*-------------------------- Constructors --------------------------*/

        public Deck()
        {
            cards = new List<Card>();
            populate();

            Dealer = new Dealer(this);

            nextClientId = 1;
            clientIndex = 0;
            callbacks = new Dictionary<int, IGameCallback>();

            listOfScores = new List<int>();
            dealerScore = 0;
            listOfWins = new List<bool>();
        }

        /*------------------ Dealer methods -----------------*/
        public void dealerHit()
        {
            updateAllClients("Dealer drew a card!");
            Dealer.drawCard(this);
        }

        public void dealerStand()
        {
            updateAllClients($"Dealer stands!\nDealer's hand:\n{Dealer.handToString()}\nDealer's score:{Dealer.CalculateHand()}");
        }

        public List<Card> dealerPrint()
        {
            return Dealer.Hand;
        }

        /*------------------ Player methods -----------------*/
        public List<Card> playerHit(int playerindex, List<Card> currentHand)
        {
            updateAllClients($"Player {playerindex} drew a card!");

            currentHand.Add(Draw());

            int score = CalculateHand(currentHand);

            if (score > 21)
            {
                updateAllClients($"Player {playerindex} went over with a score of {score}!");
            }

            return currentHand;
        }

        public void playerStand(int playerindex)
        {
            updateAllClients($"Player {playerindex} stands!");
        }

        /*------------------ Public properties and methods -----------------*/

        public int JoinGame()
        {
            // Identify which client is calling this method
            IGameCallback cb = OperationContext.Current.GetCallbackChannel<IGameCallback>();

            if (callbacks.ContainsValue(cb))
            {
                // This client is already registered, so just return the id that was 
                // assigned previously
                int i = callbacks.Values.ToList().IndexOf(cb);
                return callbacks.Keys.ElementAt(i);
            }

            // Register this client and return a new client id
            callbacks.Add(nextClientId, cb);
            updateAllClients("Player joined");
            return nextClientId++;
        }

        public void LeaveGame()
        {
            // Identify which client is calling this method
            IGameCallback cb = OperationContext.Current.GetCallbackChannel<IGameCallback>();

            if (callbacks.ContainsValue(cb))
            {
                // Identify which client is currently calling this method
                // - Get the index of the client within the callbacks collection
                int i = callbacks.Values.ToList().IndexOf(cb);
                // - Get the unique id of the client as stored in the collection
                int id = callbacks.ElementAt(i).Key;

                // Remove this client from receiving callbacks from the service
                callbacks.Remove(id);

                // Make sure the counting sequence isn't disrupted by removing this client
                if (i == clientIndex)
                    // This client was supposed to count next but is exiting the game
                    // Need to signal the next client to count instead 
                    updateAllClients("Player Left");
                else if (clientIndex > i)
                    // This prevents a player from being "skipped over" in the turn-taking
                    // of this "game"
                    clientIndex--;
            }
        }

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
        public Card Draw()
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

        Dealer IDeck.Dealer
        {
            get
            {
                return Dealer;
            }
        }

        public void EndTurn(int score)
        {
            // Determine index of the next client that gets to play
            clientIndex = ++clientIndex % callbacks.Count;

            listOfScores.Add(score);

            // Update all clients
            updateAllClients("Player Ended His Turn");

            // Increment the count and determine if the round is over
            if (++roundOverCounter == callbacks.Count)
            {
                roundOverCounter = 0;
                EndRound();
            }
        }

        public void EndRound()
        {
            updateAllClients($"\nDealer is playing...\n{Dealer.handToString()}");

            dealerScore = Dealer.CalculateHand();
            while (dealerScore < 17)
            {
                dealerHit();
                dealerScore = Dealer.CalculateHand();
            }
            dealerStand();

            // score
            foreach (int score in listOfScores)
            {
                listOfWins.Add(Dealer.PlayAgainst(score));
            }

            updateEndRound(listOfWins);

            if (++roundCounter == 3)
                gameOver = true;

            listOfScores.Clear();
            dealerScore = 0;
            listOfWins.Clear();

            populate();

            // Dealer new hand
            Dealer.Hand.Clear();
            Dealer.drawCard(this);
            Dealer.drawCard(this);
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

        public int CalculateHand(List<Card> cards)
        {
            int totalScore = 0;
            bool acePresent = false;
            foreach (Card card in cards)
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

        private void updateAllClients(string message)
        {
            foreach (IGameCallback cb in callbacks.Values)
                cb.Update(message, callbacks.Keys.ElementAt(clientIndex), gameOver);
        }

        private void updateEndRound(List<bool> list)
        {
            foreach (IGameCallback cb in callbacks.Values)
                cb.Update("", callbacks.Keys.ElementAt(clientIndex), gameOver, list);
        }

    } // end Shoe class
}
