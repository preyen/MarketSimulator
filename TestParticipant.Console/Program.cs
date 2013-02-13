using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace TestParticipant.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new HubConnection(@"http://localhost:8080/signalr");
            var hub = connection.CreateHubProxy("market");
            connection.Start().Wait();

            System.Console.WriteLine("Connected");
            hub.On("pong", () => { System.Console.WriteLine("Pong called"); });

            var r = hub.Invoke<bool>("SubscribeToDataFeed","test1").Result;

            System.Console.WriteLine(r);

            System.Console.ReadKey();
        }
    }
}
