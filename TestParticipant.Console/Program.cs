using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MarketSimulator.Agents;
using MarketSimulator.Contracts;
using MarketSimulator.LimitOrderBook;
using MarketSimulator.Utils;
using MathNet.Numerics.Distributions;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace TestParticipant.Console
{
    class Program
    {
        static LimitOrderBookSnapshot _limitOrderBookFastUpdate;
        static LimitOrderBookSnapshot _limitOrderBook;

        static void Main(string[] args)
        {
            Thread.Sleep(1000);

            var connection = new HubConnection(@"http://localhost:8080/signalr");
            var hub = connection.CreateHubProxy("market");
            connection.Start().Wait();

            System.Console.WriteLine("Connected");

            hub.On<LimitOrderBookSnapshot>("Update", data =>
                UpdateLimitOrderBook(data)
            );

            var subscribeResult = hub.Invoke("SubscribeToDataFeed", "TestDriver");
            subscribeResult.Wait();

            var orders = new List<Order>();


            orders.Add(new Order() { Price = 100, Quantity = 10, Side = OrderSide.Buy, Type = OrderType.LimitOrder,UserID="TestDriver"});
            orders.Add(new Order() { Price = 99, Quantity = 10, Side = OrderSide.Buy, Type = OrderType.LimitOrder, UserID = "TestDriver" });
            orders.Add(new Order() { Price = 98, Quantity = 20, Side = OrderSide.Buy, Type = OrderType.LimitOrder, UserID = "TestDriver" });
            orders.Add(new Order() { Price = 99.5, Quantity = 3, Side = OrderSide.Buy, Type = OrderType.LimitOrder, UserID = "TestDriver" });
            orders.Add(new Order() { Price = 102, Quantity = 10, Side = OrderSide.Sell, Type = OrderType.LimitOrder, UserID = "TestDriver" });
            orders.Add(new Order() { Price = 103, Quantity = 8, Side = OrderSide.Sell, Type = OrderType.LimitOrder, UserID = "TestDriver" });
            orders.Add(new Order() { Price = 102.5, Quantity = 19, Side = OrderSide.Sell, Type = OrderType.LimitOrder, UserID = "TestDriver" });

            foreach (var order in orders)
            {
                var r = hub.Invoke<bool>("ProcessOrderInstruction", order, "TestDriver");
                r.Wait();
            }

            Thread.Sleep(5);

            var rng = new CSharpRandomNumberGenerator() as IRandomNumberGenerator;

            var agents = new List<IAgent>();

            var normalDist = new Normal(0,0.5);

            for (int i = 0; i < 200; i++)
            {
                agents.Add(new RandomLiquidityMaker(rng, 30, 2, 0.3,normalDist,4));
            }

            for (int i = 0; i < 200; i++)
            {
                agents.Add(new RandomLiquidityTaker(rng, 10, 0.8));
            }

            agents.Add(new BigOrderAgent());

            _limitOrderBook = _limitOrderBookFastUpdate;

            for (int i = 0; i < 1000; i++)
            {
                agents.Shuffle();

                var count = 0;
                foreach (var agent in agents)
                {
                    var order = agent.GetNextAction(_limitOrderBook);
                    if (order != null)
                    {
                        order.UserID = "testAgent" + count.ToString();
                        var r = hub.Invoke<bool>("ProcessOrderInstruction", order, "testAgent" + count.ToString());
                        r.Wait();
                    }
                    count++;
                }
                _limitOrderBook = _limitOrderBookFastUpdate;
                if (_limitOrderBook.BestAskPrice < _limitOrderBook.BestBidPrice)
                {
                    throw new Exception("Book crossed");
                }

                //Thread.Sleep(5000);
                System.Console.WriteLine(_limitOrderBook.BestBidPrice + "\t\t\t" + _limitOrderBook.BestAskPrice);
               
            }          

            System.Console.ReadKey();
        }

        private static void UpdateLimitOrderBook(LimitOrderBookSnapshot lob)
        {
            if (lob.BestAskPrice != null)
            {
                lob.BestAskPrice = lob.BestAskPrice ?? 102;
                lob.BestBidPrice = lob.BestBidPrice ?? 100;
                _limitOrderBookFastUpdate = lob;
            }
            //System.Console.WriteLine(_limitOrderBookFastUpdate.BestBidPrice + "\t\t\t" + _limitOrderBookFastUpdate.BestAskPrice);
        }
    }

    public static class ExtensionMethods
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

    }
}

