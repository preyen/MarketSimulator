using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using System.Drawing;
using System.Threading;
using Microsoft.AspNet.SignalR.Client.Hubs;
using MarketSimulator.Contracts;
using System.IO;
using HetroTradingRules.TestParticipant.Console.Solvers;

namespace HetroTradingRules.TestParticipant.Console
{
    class Program
    {
        private const int NumberOfAgents = 5000;
        private const int MaxNumberOfStock = 50;

        private const double FundamentalValueInitial = 300;
        private const double FundamentalValueDrift = 0;
        private const double FundamentalValueVariance = 10E-4;// 0.001;

        private const double MaxCash = MaxNumberOfStock * FundamentalValueInitial;
        private const double ReferenceAgentTimeHorizon = 200;
        private const double TickSize = 0.0005;
        private const int SimulationSteps = 5000;

        private const double NoiseVariance = 0.0001;

        private const double ReferenceRiskAversionLevel = 0.01;

        private static LimitOrderBookSnapshot _currentLimitOrderBook;
        
        private static HetroTradingRulesAgent[] _agents;


        static void Main(string[] args)
        {
            var randomGenerator = new Random();

            var fundamentalValue = GenerateFunamentalValuePath(SimulationSteps, FundamentalValueInitial, FundamentalValueDrift, FundamentalValueVariance);

            var fundamentalistScale = 10;
            var chartistScale = 1.2;
            var noiseScale = 1;

            var fundamentalistDistribution = new Laplace(0, fundamentalistScale);
            fundamentalistDistribution.RandomSource = randomGenerator;
            var chartistDistribution = new Laplace(0, chartistScale);
            chartistDistribution.RandomSource = randomGenerator;
            var noiseDistribution = new Laplace(0, noiseScale);
            noiseDistribution.RandomSource = randomGenerator;

            var normal = new Normal(0, 5);

            _agents = new HetroTradingRulesAgent[NumberOfAgents];

            var solver = new Brent();

            for (int i = 0; i < NumberOfAgents; i++)
            {
                _agents[i] = new HetroTradingRulesAgent(Math.Abs(fundamentalistDistribution.Sample()), Math.Abs(chartistDistribution.Sample()), Math.Abs(noiseDistribution.Sample()), ReferenceAgentTimeHorizon, ReferenceRiskAversionLevel, randomGenerator, solver);
                _agents[i].StockHeld = (int)Math.Floor(MaxNumberOfStock * randomGenerator.NextDouble());
             
                _agents[i].CashHeld = Math.Round(MaxNumberOfStock * FundamentalValueInitial * randomGenerator.NextDouble(), 2);
            }

            Thread.Sleep(1000);

            var connection = new HubConnection(@"http://localhost:8080/signalr");
            var hub = connection.CreateHubProxy("market");
            connection.Start().Wait();

            System.Console.WriteLine("Connected");

            hub.On<LimitOrderBookSnapshot>("Update", data =>
                UpdateLimitOrderBook(data)
            );

            hub.On<OrderUpdate>("UpdateOrder", update => UpdateAgent(update));

            var subscribeResult = hub.Invoke("SubscribeToDataFeed", "TestDriver");
            subscribeResult.Wait();

            var spotPrice = new double[SimulationSteps];

            var stepsToFill = 1000;

            for (int i = 0; i < stepsToFill; i++)
            {
                spotPrice[i] = fundamentalValue[i];
            }

            //spotPrice[0] = fundamentalValue[0];
            //spotPrice[1] = spotPrice[0] * 1.0001;
            //spotPrice[2] = spotPrice[1] * 1.0002;
            //spotPrice[3] = spotPrice[2] * 1.0003;

            var noise = GenerateNoisePath(SimulationSteps, 0, NoiseVariance);

            var outputFile = @"c:\temp\hetroTradingRulesNoiseFundChart.csv";

            File.WriteAllLines(outputFile, new string[] { string.Format("{0},{1},{2}", "TimeStep", "Price", "Fundamental") });

            for (int i = stepsToFill; i < SimulationSteps - ReferenceAgentTimeHorizon; i++)
            {
                var agentIndex = (int)Math.Round((NumberOfAgents - 1) * randomGenerator.NextDouble());
                Order order = null;
                try
                {
                    order = _agents[agentIndex].GetAction(i, spotPrice, fundamentalValue, noise, _currentLimitOrderBook != null ? _currentLimitOrderBook.BestBidPrice : null, _currentLimitOrderBook != null ? _currentLimitOrderBook.BestAskPrice : null);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("ERROR: {0}", e.Message);
                }

                if (order != null)
                {
                    order.UserID = agentIndex.ToString();
                    var r = hub.Invoke<bool>("ProcessOrderInstruction", order, agentIndex.ToString());
                    r.Wait();
                    Thread.Sleep(10);
                    if (order.Type == OrderType.MarketOrder)
                    {
                        spotPrice[i] = order.Price;
                    }
                    else
                        if (_currentLimitOrderBook != null && _currentLimitOrderBook.BestAskPrice.HasValue && _currentLimitOrderBook.BestBidPrice.HasValue)
                        {
                            spotPrice[i] = (_currentLimitOrderBook.BestBidPrice.Value + _currentLimitOrderBook.BestAskPrice.Value) / 2;
                        }
                        //else if (_currentLimitOrderBook != null && _currentLimitOrderBook.BestAskPrice.HasValue)
                        //{
                        //    spotPrice[i] = _currentLimitOrderBook.BestAskPrice.Value;
                        //}
                        //else if (_currentLimitOrderBook != null && _currentLimitOrderBook.BestBidPrice.HasValue)
                        //{
                        //    spotPrice[i] = _currentLimitOrderBook.BestBidPrice.Value;
                        //}
                        else
                        {
                            spotPrice[i] = spotPrice[i - 1];
                        }
                }
                else
                {
                    spotPrice[i] = spotPrice[i - 1];
                }
                File.AppendAllLines(outputFile, new string[] { string.Format("{0},{1},{2}", i, spotPrice[i], fundamentalValue[i]) });
                System.Console.WriteLine(string.Format("{0}\t\t{1}",i,spotPrice[i]));
            }
        }

        private static void UpdateAgent(OrderUpdate update)
        {
            var agentIndex = int.Parse(update.Order.UserID);

            switch (update.Order.Type)
            {
                case OrderType.LimitOrder:
                    if (update.Matches != null)
                    {
                        switch (update.Order.Side)
                        {
                            case OrderSide.Buy:
                                _agents[agentIndex].StockHeld += (int)update.Matches.Sum(m => m.Quantity);
                                _agents[agentIndex].CashHeld -= update.Matches.Sum(m => m.Price * m.Quantity);
                                break;
                            case OrderSide.Sell:
                                _agents[agentIndex].StockHeld -= (int)update.Matches.Sum(m => m.Quantity);
                                _agents[agentIndex].CashHeld += update.Matches.Sum(m => m.Price * m.Quantity);
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                //case OrderType.MarketOrder:
                //    switch (update.Order.Side)
                //    {
                //        case OrderSide.Buy:
                //            _agents[agentIndex].StockHeld += (int)update.Order.Quantity;
                //            _agents[agentIndex].CashHeld -= update.Order.Quantity * update.Order.Price;
                //            break;
                //        case OrderSide.Sell:
                //            _agents[agentIndex].StockHeld -= (int)update.Order.Quantity;
                //            _agents[agentIndex].CashHeld += update.Order.Quantity * update.Order.Price;
                //            break;
                //        default:
                //            break;
                //    }
                //    break;
                default:
                    break;
            }
        }

        private static void UpdateLimitOrderBook(LimitOrderBookSnapshot data)
        {
            if (data != null)
            {
                _currentLimitOrderBook = data;
            }
        }

        private static double[] GenerateNoisePath(int simulationSteps, int mean, double noiseVariance)
        {
            var standardNormal = new Normal(mean, noiseVariance);

            return standardNormal.Samples().Take(simulationSteps).ToArray();
        }

        private static double[] GenerateFunamentalValuePath(int simulationSteps, double fundamentalValueInitial, double fundamentalValueDrift, double fundamentalValueVariance)
        {
            Normal standardNormal = new Normal(0, 1);

            var fundamentalValue = new double[simulationSteps];

            fundamentalValue[0] = fundamentalValueInitial;

            for (int i = 1; i < simulationSteps; i++)
            {
                fundamentalValue[i] = CalculateNextValue(1, fundamentalValue[i-1], fundamentalValueDrift, fundamentalValueVariance, standardNormal);
            }

            return fundamentalValue;
        }

        private static double CalculateNextValue(double timeStep, double currentValue, double drift, double variance, Normal normalDistribution)
        {
            return currentValue * Math.Exp(((drift-(0.5*Math.Pow(variance,2)))*timeStep) + variance*Math.Sqrt(timeStep)*normalDistribution.Sample());
        }
    }
}
