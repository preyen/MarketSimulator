using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Contracts;
using MathNet.Numerics.Distributions;

namespace HetroTradingRules.TestParticipant.Console
{
    public class HetroTradingRulesAgent
    {
        public int StockHeld { get; set; }
        public double CashHeld { get; set; }

        private double _fundamentalistWeighting;
        private double _chartistWeighting;
        private double _noiseWeighting;
        private int _agentTimeHorizon;
        private double _agentRiskAversionLevel;
        private Random _randomGenerator;

        public HetroTradingRulesAgent(double fundementalistWeighting, double chartistWeighting, double noiseWeighting, double referenceAgentTimeHorizon, double referenceRiskAversionLevel, Random randomGenerator)
        {
            _fundamentalistWeighting = fundementalistWeighting;
            _chartistWeighting = chartistWeighting;
            _noiseWeighting = noiseWeighting;
            _agentTimeHorizon = (int) Math.Floor(referenceAgentTimeHorizon * ((1 + fundementalistWeighting) / (1 + chartistWeighting)));
            _agentRiskAversionLevel = referenceRiskAversionLevel * ((1 + fundementalistWeighting) / (1 + chartistWeighting));
            _randomGenerator = randomGenerator;
        }

        public Order GetAction(int timeStep, double[] spotPrice, double[] fundamentalValue, double[] noise, double? bestBid, double? bestAsk) 
        {
            var fundamentalTimeHorizon = _agentTimeHorizon;

            var normalisationTerm = 1/(_fundamentalistWeighting + _chartistWeighting + _noiseWeighting);

            var fundamentalistTerm = _fundamentalistWeighting * (1/fundamentalTimeHorizon) * Math.Log(fundamentalValue[timeStep]/spotPrice[timeStep]);

            var averageReturn = (1 / _agentTimeHorizon) * Enumerable.Range(1,_agentTimeHorizon).Aggregate(0d,(curr,j) => curr + Math.Log(spotPrice[timeStep-j] / spotPrice[timeStep-j-1]));
            var chartistTerm = _chartistWeighting * averageReturn;

            var noiseTerm = _noiseWeighting * noise[timeStep];


            var expectedReturn = normalisationTerm * (fundamentalistTerm + chartistTerm + noiseTerm);

            var expectedPrice = spotPrice[timeStep] * Math.Exp(expectedReturn * _agentTimeHorizon);

            var varianceOfPastReturns = (1/_agentTimeHorizon) * Enumerable.Range(1,_agentTimeHorizon).Aggregate(0d,(curr,j) => curr + Math.Pow(((Math.Log(spotPrice[timeStep-j] / spotPrice[timeStep-j-1])) - averageReturn),2));                        

            var comfortPriceAtCurrentHolding = FindRoot(p => GetStocksToHold(p,expectedPrice,_agentRiskAversionLevel,varianceOfPastReturns) - StockHeld,spotPrice[timeStep],0.001);

            var maxPrice = expectedPrice;
            var minPrice = FindRoot(p => p * (GetStocksToHold(p, expectedPrice, _agentRiskAversionLevel, varianceOfPastReturns) - StockHeld) - StockHeld, maxPrice, 0.001);

            var drawnPrice = minPrice + ((maxPrice-minPrice) * _randomGenerator.NextDouble());

            var order = new Order();

            if (drawnPrice < comfortPriceAtCurrentHolding)
            {
                //buy
                order.Side = OrderSide.Buy;

                if (bestAsk.HasValue && drawnPrice > bestAsk.Value)
                {
                    //buy market order
                    order.Quantity = GetStocksToHold(bestAsk.Value, expectedPrice, _agentRiskAversionLevel, varianceOfPastReturns) - StockHeld;
                    order.Type = OrderType.MarketOrder;                    
                }
                else
                {
                    //buy limit order at drawnPrice
                    order.Quantity = GetStocksToHold(drawnPrice, expectedPrice, _agentRiskAversionLevel, varianceOfPastReturns) - StockHeld;
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
                    order.Quantity = StockHeld - GetStocksToHold(bestBid.Value, expectedPrice, _agentRiskAversionLevel, varianceOfPastReturns);
                }
                else
                {
                    //sell limit order at drawnProce
                    order.Quantity = StockHeld - GetStocksToHold(drawnPrice, expectedPrice, _agentRiskAversionLevel, varianceOfPastReturns);
                    order.Type = OrderType.LimitOrder;
                    order.Price = drawnPrice;
                }
                
            }
            else
            {
                //do nothing
                order = null;
            }

            return order;

            //TODO: keep track of orders
        }

        public double GetPortfolioWealth(double spotPrice)
        {
            return (StockHeld * spotPrice) + CashHeld;
        }

        public double GetStocksToHold(double spotPrice,double expectedPrice,double riskAversionLevel,double varianceOfPastReturns)
        {
            return Math.Floor((Math.Log(expectedPrice / spotPrice) / (riskAversionLevel * varianceOfPastReturns * spotPrice)));
        }

        public double FindRoot(Func<double, double> function, double initialGuess, double error)
        {
            return initialGuess;            
        }
    }
}

