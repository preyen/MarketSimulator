using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Contracts;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace TestParticipantDataFeed.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new HubConnection(@"http://localhost:8080/signalr");
            var hub = connection.CreateHubProxy("market");
            connection.Start().Wait();

            System.Console.WriteLine("Connected");
            hub.On("Update", data => System.Console.WriteLine("BestBid: " + data.BestBid.Price + "BestAsk: " + data.BestAsk.Price));
            

            var r = hub.Invoke<bool>("SubscribeToDataFeed","test2");
            r.Wait();
            
            System.Console.WriteLine("Subscribe result: " + r.Result);

            System.Console.ReadKey();
        }
    }
}
