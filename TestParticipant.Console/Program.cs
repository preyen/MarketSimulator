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
            var connection = new HubConnection(@"http://Preyen-PC:8088/");
            var hub = connection.CreateHubProxy("MarketCommunications");
            connection.Start().Wait();
            

            hub.On("Pong", s => System.Console.WriteLine(s));

            hub.Invoke("Ping");

            System.Console.ReadKey();
        }
    }
}
