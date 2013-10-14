using System;
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
            int NumberOfInformedAgents = 100;
            int NumberOfUninformedAgents = 100;
            int NumberOfDays = 2250;
            int NumberOfTradingPeriods = 8;
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

            var agents = new List<IAgent>();

            //init infrormed agents
            for (int i = 0; i < NumberOfInformedAgents; i++)
            {
                var agent = new InformedAgent(random,100,"Agent" + i);
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
                //loop through trading periods
                for (int j = 0; j < NumberOfTradingPeriods; j++)
                {
                    //shuffle agents
                    agents.Shuffle();

                    //get next action from each agent
                    foreach (var agent in agents)
                    {
                        var ordersToCancel = agent.GetOrdersToCancel(i, j);
                        
                        var order = agent.GetNextAction(_limitOrderBookSnapshot, i, j);
                    }
                }
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
