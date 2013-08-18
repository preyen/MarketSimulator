using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HetroTradingRules.TestParticipant.Console.Solvers;
using MarketSimulator.Contracts;
using MathNet.Numerics.Distributions;

namespace HetroTradingRules.TestParticipant.Console
{
    public class HetroTradingRulesAgent
    {
        public int StockHeld { get; set; }
        public double CashHeld { get; set; }

        private decimal _fundamentalistWeighting;
        private decimal _chartistWeighting;
        private decimal _noiseWeighting;
        private int _agentTimeHorizon;
        private decimal _agentRiskAversionLevel;
        private Random _randomGenerator;
        private Normal _normal;
        private Solver1D _solver;

        public HetroTradingRulesAgent(double fundementalistWeighting, double chartistWeighting, double noiseWeighting, double referenceAgentTimeHorizon, double referenceRiskAversionLevel, Random randomGenerator, Solver1D solver)
        {
            _fundamentalistWeighting = (decimal)fundementalistWeighting;
            _chartistWeighting = (decimal)chartistWeighting;
            _noiseWeighting = (decimal)noiseWeighting;
            _agentTimeHorizon = (int) Math.Floor(referenceAgentTimeHorizon * ((1 + fundementalistWeighting) / (1 + chartistWeighting)));
            _agentRiskAversionLevel = (decimal) (referenceRiskAversionLevel * ((1 + fundementalistWeighting) / (1 + chartistWeighting)));
            _randomGenerator = randomGenerator;
            _solver = solver;
        }

        public Order GetAction(int timeStep, double[] spotPrice, double[] fundamentalValue, double[] noise, double? bestBid, double? bestAsk) 
        {            
            var fundamentalTimeHorizon = _agentTimeHorizon;
            var lookBackTime = Math.Min(_agentTimeHorizon, timeStep)-1;

            var normalisationTerm = _fundamentalistWeighting + _chartistWeighting + _noiseWeighting;

            var fundamentalistTerm = (_fundamentalistWeighting * (decimal)Math.Log(fundamentalValue[timeStep - 1] / spotPrice[timeStep - 1])) / fundamentalTimeHorizon; //should the fundamental TH be added??           


            var averageReturn = CalculateAverageReturn(timeStep, spotPrice, lookBackTime);
            var chartistTerm = _chartistWeighting * averageReturn;

            var noiseTerm = _noiseWeighting * (decimal) noise[timeStep];


            var expectedReturn = (fundamentalistTerm + chartistTerm + noiseTerm) / normalisationTerm;

            var expectedPrice = Math.Round(spotPrice[timeStep-1] * Math.Exp((double)(expectedReturn * _agentTimeHorizon)),4);

            var varianceOfPastReturns = CalculateVarianceOfPastReturns(timeStep, spotPrice, lookBackTime, averageReturn);

           // var stockToHoldAtExpected = GetStocksToHold(expectedPrice/2, expectedPrice, (double)_agentRiskAversionLevel, (double)varianceOfPastReturns);
           // if (stockToHoldAtExpected <= 1) return null;// throw new Exception("Cannot short sell!!!");
            

            var comfortPriceAtCurrentHolding = Math.Round(FindRoot(p => GetStocksToHold(p, expectedPrice, (double)_agentRiskAversionLevel, (double)varianceOfPastReturns) - StockHeld, expectedPrice, 0.0000001,0.01,expectedPrice), 4);

            var maxPrice = expectedPrice;
            var minPrice = FindRoot(p => p * (GetStocksToHold(p, expectedPrice, (double)_agentRiskAversionLevel, (double)varianceOfPastReturns) - StockHeld) - CashHeld, comfortPriceAtCurrentHolding, 0.0000001,0.1,expectedPrice);

            var drawnPrice = Math.Round(Math.Min(minPrice,maxPrice) + (Math.Abs(maxPrice-minPrice) * _randomGenerator.NextDouble()),4);

            var order = new Order();

            if (drawnPrice < comfortPriceAtCurrentHolding)
            {
                //buy
                order.Side = OrderSide.Buy;

                if (bestAsk.HasValue && drawnPrice > bestAsk.Value)
                {
                    //buy market order
                    order.Quantity = GetStocksToHold(bestAsk.Value, expectedPrice, (double)_agentRiskAversionLevel, (double)varianceOfPastReturns) - StockHeld;
                    order.Type = OrderType.MarketOrder;
                    order.Price = bestAsk.Value;
                    StockHeld += (int)order.Quantity;
                    CashHeld -= order.Quantity * order.Price;
                }
                else
                {
                    //buy limit order at drawnPrice
                    order.Quantity = GetStocksToHold(drawnPrice, expectedPrice, (double)_agentRiskAversionLevel, (double)varianceOfPastReturns) - StockHeld;
                    order.Type = OrderType.LimitOrder;
                    order.Price = drawnPrice;
                }
            }
            else if (drawnPrice > comfortPriceAtCurrentHolding)
            {
                //sell
                order.Side = OrderSide.Sell;

                if (bestBid.HasValue && drawnPrice < bestBid.Value)
                {
                    //sell market order
                    order.Quantity = StockHeld - GetStocksToHold(bestBid.Value, expectedPrice, (double)_agentRiskAversionLevel, (double)varianceOfPastReturns);
                    order.Type = OrderType.MarketOrder;
                    order.Price = bestBid.Value;
                    StockHeld -= (int)order.Quantity;
                    CashHeld += order.Quantity * order.Price;
                }

                else
                {
                    //sell limit order at drawnProce
                    order.Quantity = StockHeld - GetStocksToHold(drawnPrice, expectedPrice, (double)_agentRiskAversionLevel, (double)varianceOfPastReturns);
                    order.Type = OrderType.LimitOrder;
                    order.Price = drawnPrice;
                }
                
            }
            else
            {
                //do nothing
                order = null;
            }

            //if (order != null && ((order.Price - spotPrice[timeStep-1]) / spotPrice[timeStep-1]) > 0.1)
            //{
            //    order = null;
            //}

            return order;

            //TODO: keep track of orders
        }

        private static decimal CalculateAverageReturn(int timeStep, double[] spotPrice, int lookBackTime)
        {
            var total = 0m;
            var count = 0;

            for (int i = 1; i <= lookBackTime; i++)
            {
                count++;

                total += (decimal) Math.Log(spotPrice[timeStep - i] / spotPrice[timeStep - i - 1]);
            }

            return total / count;

            //return Enumerable.Range(1, lookBackTime).Aggregate(0m, (curr, j) => curr + (((decimal)Math.Log(spotPrice[timeStep - j] / spotPrice[timeStep - j - 1])) / lookBackTime));
        }

        private static decimal CalculateVarianceOfPastReturns(int timeStep, double[] spotPrice, int lookBackTime, decimal averageReturn)
        {
            var total = 0m;
            var count = 0;

            for (int i = 1; i <= lookBackTime; i++)
            {
                count++;
                total += (decimal) Math.Pow((double)((decimal)Math.Log(spotPrice[timeStep - i] / spotPrice[timeStep - i - 1]) - averageReturn), 2);
            }

            return total / count;

            //return Enumerable.Range(1, lookBackTime).Aggregate(0m, (curr, j) => curr + (((decimal)Math.Pow(((Math.Log((double)spotPrice[timeStep - j] / (double)spotPrice[timeStep - j - 1])) - (double)averageReturn), 2)) / lookBackTime));
        }

        public double GetPortfolioWealth(double spotPrice)
        {
            return (StockHeld * spotPrice) + CashHeld;
        }

        public int GetStocksToHold(double spotPrice,double expectedPrice,double riskAversionLevel,double varianceOfPastReturns)
        {
            return (int) Math.Floor((Math.Log(expectedPrice / spotPrice) / (riskAversionLevel * varianceOfPastReturns * spotPrice)));
        }

        public double FindRoot(Func<double, double> function, double initialGuess, double error)
        {
            return _solver.solve(function, error, initialGuess, 0.00001);
        }

        public double FindRoot(Func<double, double> function, double initialGuess, double error, double lowerBound, double upperBound)
        {
            return _solver.solve(function, error, initialGuess, lowerBound, upperBound);
        }
    }
}

