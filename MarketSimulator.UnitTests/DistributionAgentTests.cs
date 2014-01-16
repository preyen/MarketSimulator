using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Agents;
using MarketSimulator.Contracts;
using MarketSimulator.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.Distributions;
using System.IO;

namespace TestParticipant.Console
{
   // [TestClass]
    public class DistributionAgentTests
    {
     //   [TestMethod]
        public void GetPriceDistribution()
        {
            var lob = new LimitOrderBookSnapshot();
            lob.BestAskPrice = 101;
            lob.BestBidPrice = 99;

            var buyPrices = new SortedDictionary<double, int>();
            var sellPrices = new SortedDictionary<double, int>();

            var agent = new RandomLiquidityMaker(new CSharpRandomNumberGenerator(), 100, 10, 0, new Normal(0,0.2),2);

            for (int i = 0; i < 100000000; i++)
            {
                var order = agent.GetNextAction(lob);

                if (order.Side == OrderSide.Buy)
                {
                    if (!buyPrices.ContainsKey(order.Price))
                    {
                        buyPrices.Add(order.Price, 0);
                    }

                    buyPrices[order.Price]++;
                }
                else
                {
                    if (!sellPrices.ContainsKey(order.Price))
                    {
                        sellPrices.Add(order.Price, 0);
                    }

                    sellPrices[order.Price]++;
                }
            }

            var lines = new List<string>();

            foreach (var price in buyPrices)
            {
                lines.Add(string.Format("{0},{1}", price.Key, price.Value));
            }

            foreach (var price in sellPrices)
            {
                lines.Add(string.Format("{0},{1}", price.Key, price.Value));
            }

            File.WriteAllLines(@"c:\temp\test.csv", lines);
        }
    }
}
