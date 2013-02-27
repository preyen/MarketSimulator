using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Contracts;
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

            var order = new Order();            
            order.Quantity = 4;
            order.Price = 100;
            order.Type = OrderType.LimitOrder;
            order.Side = OrderSide.Buy;
            order.UserID = "test1";

            var r = hub.Invoke<bool>("ProcessOrderInstruction", order, "test1");
            r.Wait();


            order = new Order();
            order.Quantity = 4;
            order.Price = 101;
            order.Type = OrderType.LimitOrder;
            order.Side = OrderSide.Sell;
            order.UserID = "test1";

            r = hub.Invoke<bool>("ProcessOrderInstruction", order, "test1");
            r.Wait();

            order = new Order();
            order.Quantity = 2;
            order.Type = OrderType.MarketOrder;
            order.Side = OrderSide.Buy;
            order.UserID = "test1";

            r = hub.Invoke<bool>("ProcessOrderInstruction", order, "test1");
            r.Wait();

                       

            System.Console.WriteLine(r.Result);

            System.Console.ReadKey();
        }
    }
}
