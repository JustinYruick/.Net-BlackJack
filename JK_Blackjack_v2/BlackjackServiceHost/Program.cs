// Coders: Justin Yurick & Kiril Trenkov
// Date: April 11th, 2021

using System;
using System.ServiceModel;
using BlackjackLibrary;

namespace BlackjackServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost servHost = null;

            try
            {
                // Register the service Address
                //servHost = new ServiceHost(typeof(Deck), new Uri("net.tcp://localhost:13200/BlackjackLibrary/"));

                // Register the service Contract and Binding
                //servHost.AddServiceEndpoint(typeof(IDeck), new NetTcpBinding(), "DeckService");
                //servHost.AddServiceEndpoint(typeof(IDealer), new NetTcpBinding(), "DealerService");

                servHost = new ServiceHost(typeof(Deck));

                // Run the service
                servHost.Open();

                Console.WriteLine("Service started. Press any key to quit.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // Wait for a keystroke
                Console.ReadKey();
                servHost?.Close();
            }
        }
    }
}
