using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MarketSimulator.Agents;
using MarketSimulator.Contracts;
using MarketSimulator.Utils;
using MathNet.Numerics.Distributions;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace RealTime.TestParticipant.Console
{
    class Program
    {
        static LimitOrderBookSnapshot _lob;

        static void Main(string[] args)
        {
            System.IO.File.WriteAllLines(@"c:\temp\test.csv", new string[] {"Time,BestBid,BestAsk"});
            var providers = new List<LiquidityProvider>();

            var rng = new CSharpRandomNumberGenerator() as IRandomNumberGenerator;
            var normalDist = new Normal(0, 0.01);

            var numLiquidityProviders = 10;
            var numLiquidityTakers = 10;

            for (int i = 0; i < numLiquidityProviders; i++)
            {
                providers.Add(new LiquidityProvider(rng,50,1,0.3,normalDist,4));
            }

            var takers = new List<RandomLiquidityTaker>();

            for (int i = 0; i < numLiquidityTakers; i++)
            {
                takers.Add(new RandomLiquidityTaker(rng, 5, 0.6));
            }

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


            orders.Add(new Order() { Price = 100, Quantity = 10, Side = OrderSide.Buy, Type = OrderType.LimitOrder, UserID = "TestDriver" });
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

            while (true)
            {
                Thread.Sleep(10);

                var rndLT = rng.GetRandomInt(0, numLiquidityTakers - 1);
                var rndLP = rng.GetRandomInt(0, numLiquidityProviders - 1);

                var rnd = rng.GetRandomInt(1, 100);

                Order[] orders2; 

                if (rnd % 2 == 0)
                {
                    orders2 = providers[rndLP].GetNextActions(_lob);
                }
                else
                {
                    orders2 = new Order[] { takers[rndLT].GetNextAction(_lob) };
                }

                foreach (var item in orders2)
                {
                    if (item != null)
                    {
                        item.UserID = "TestDriver";
                        var r = hub.Invoke<bool>("ProcessOrderInstruction", item, "TestDriver");
                        r.Wait();
                    }
                }
            }


        }

        private static void UpdateLimitOrderBook(LimitOrderBookSnapshot data)
        {
            if (data.BestAskPrice != null)
            {
                data.BestAskPrice = data.BestAskPrice ?? _lob.BestAskPrice;
                data.BestBidPrice = data.BestBidPrice ?? _lob.BestBidPrice;
                _lob = data;
                System.Console.WriteLine(data.BestBidPrice + "\t\t\t" + data.BestAskPrice);
                System.IO.File.AppendAllLines(@"c:\temp\test.csv", new string[] { DateTime.Now.Ticks + "," + data.BestBidPrice + "," + data.BestAskPrice });
            }
        }
    }
}
