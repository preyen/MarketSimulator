using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Contracts;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace IntradayTradingPatterns.TestParticipant.Console
{
    class Program
    {
        private static LimitOrderBookSnapshot _limitOrderBookSnapshot;

        static void Main(string[] args)
        {
            System.Threading.Thread.Sleep(1000);

            int NumberOfInformedAgents = 40;
            int NumberOfUninformedAgents = 160;
            int NumberOfDays = 2250;
            int NumberOfTradingPeriods = 8;
            double MinExpectedAssetValue = 20;
            double MaxExpectedAssetValue = 120;
            double MinExpectedAssetValueRange = 5;
            double MaxExpectedAssetValueRange = 15;
            int NumberOfDaysInLearningPeriod = 15;
            bool InformedAgentsCompete = true;
            int NumberOfAgentsInGroup = 40;

            var geneticSelection = Enumerable.Range(0, NumberOfAgentsInGroup).ToList();

            var random = new Random();

            var connection = new HubConnection(@"http://localhost:8080/signalr");
            var hub = connection.CreateHubProxy("market");
            connection.Start().Wait();

            System.Console.WriteLine("Connected");

            hub.On<LimitOrderBookSnapshot>("Update", data =>
                UpdateLimitOrderBook(data)
            );

            var subscribeResult = hub.Invoke("SubscribeToDataFeed", "TestDriver");
            subscribeResult.Wait();

            var agents = new List<Agent>();

            //init infrormed agents
            for (int i = 0; i < NumberOfInformedAgents; i++)
            {
                var agent = new InformedAgent(random,100,"Agent" + i,InformedAgentsCompete);
                agents.Add(agent);
            }

            //init uninformed agents
            for (int i = 0; i < NumberOfUninformedAgents; i++)
            {
                var agent = new UninformedAgent(random, 100, "Agent" + i);
                agents.Add(agent);
            }


            //loop through days
            for (int i = 0; i < NumberOfDays; i++)
            {
                var assetValue = MinExpectedAssetValue + random.NextDouble() * (MaxExpectedAssetValue-MinExpectedAssetValue);
                //loop through trading periods
                for (int j = 0; j < NumberOfTradingPeriods; j++)
                {
                    //shuffle agents
                    agents.Shuffle();

                    //get next action from each agent
                    foreach (var agent in agents)
                    {
                        if (agent is InformedAgent)
                        {
                            agent.ExpectedAssetLiquidationValue = assetValue;
                        }
                        else
                        {
                            agent.ExpectedAssetLiquidationValue = MinExpectedAssetValue + random.NextDouble() * (MaxExpectedAssetValue-MinExpectedAssetValue);
                        }

                        agent.ExpectedAssetLiquidationValueOrderRange = MinExpectedAssetValueRange + random.NextDouble() * (MaxExpectedAssetValueRange - MinExpectedAssetValueRange);

                        var ordersToCancel = agent.GetOrdersToCancel(i, j);
                        
                        var order = agent.GetNextAction(_limitOrderBookSnapshot, i, j);

                        if (order != null)
                        {
                            var r = hub.Invoke<bool>("ProcessOrderInstruction", order, agent.Name);
                            r.Wait();
                            //set agents profit/loss
                            if (order.Side == OrderSide.Buy)
                            {
                                agent.CurrentProfit += (assetValue - order.Price) * order.Quantity;
                            }
                            else if (order.Side == OrderSide.Sell)
                            {
                                agent.CurrentProfit += (order.Price - assetValue) * order.Quantity;
                            }
                        }
                    }

                    System.Console.WriteLine(string.Format("{0} {1} {2} {3}", i,j,_limitOrderBookSnapshot.BestBidPrice,_limitOrderBookSnapshot.BestAskPrice));
                }

                if ((i + 1) % NumberOfDaysInLearningPeriod == 0)
                {
                    foreach (var agent in agents)
                    {
                        geneticSelection.Shuffle();

                        var selectedAgents = new List<Agent>();

                        for (int j = 0; j < geneticSelection.Count; j++)
                        {
                            selectedAgents.Add(agents[geneticSelection[j]]);
                        }

                        agent.EvolveTimingChromosome(selectedAgents);
                    }

                    foreach (var agent in agents)
                    {
                        agent.CurrentProfit = 0;
                    }
                }

                //clear trading book every day
                hub.Invoke("ClearAllTrades");
            }            
        }

        private static void UpdateLimitOrderBook(LimitOrderBookSnapshot data)
        {
            _limitOrderBookSnapshot = data;
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

