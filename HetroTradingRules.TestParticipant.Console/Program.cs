﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Distributions;
using System.Drawing;

namespace HetroTradingRules.TestParticipant.Console
{
    class Program
    {
        private const int NumberOfAgents = 5000;
        private const int MaxNumberOfStock = 50;

        private const double FundamentalValueInitial = 300;
        private const double FundamentalValueDrift = 0;
        private const double FundamentalValueVariance = 0.001;

        private const double MaxCash = MaxNumberOfStock * FundamentalValueInitial;
        private const double ReferenceAgentTimeHorizon = 200;
        private const double TickSize = 0.0005;
        private const int SimulationSteps = 200000;

        private const double NoiseDensityVariance = 1;
        private const double NoiseVariance = 0.0001;

        private const double ReferenceRiskAversionLevel = 0.1;        


        static void Main(string[] args)
        {           
            var fundamentalValue = GenerateFunamentalValuePath(SimulationSteps,FundamentalValueInitial,FundamentalValueDrift,FundamentalValueVariance);                      

            var fundamentalistScale = 0;
            var chartistScale = 0;
            var noiseScale = NoiseDensityVariance;

            var fundamentalistDistribution = new Laplace(0, fundamentalistScale);
            var chartistDistribution = new Laplace(0, chartistScale);
            var noiseDistribution = new Laplace(0, noiseScale);
            var agentDistribution = new Random();

            var agents = new HetroTradingRulesAgent[NumberOfAgents];

            for (int i = 0; i < NumberOfAgents; i++)
            {
                agents[i] = new HetroTradingRulesAgent(fundamentalistDistribution.Sample(), chartistDistribution.Sample(), noiseDistribution.Sample(), ReferenceAgentTimeHorizon, ReferenceRiskAversionLevel);
            }

            var spotPrice = new double[SimulationSteps];
            spotPrice[0] = FundamentalValueInitial;

            var noise = GenerateNoisePath(SimulationSteps,0, NoiseVariance);

            for (int i = 0; i < SimulationSteps; i++)
            {
                var agentIndex = (int) Math.Round(NumberOfAgents * agentDistribution.NextDouble());
                agents[agentIndex].GetAction(i,spotPrice, fundamentalValue, noise);
            }
        }

        private static double[] GenerateNoisePath(int simulationSteps, int mean, double noiseVariance)
        {
            var standardNormal = new Normal(mean, noiseVariance);

            return standardNormal.Samples().Take(simulationSteps).ToArray();
        }

        private static double[] GenerateFunamentalValuePath(int simulationSteps, double fundamentalValueInitial, double fundamentalValueDrift, double fundamentalValueVariance)
        {
            Normal standardNormal = new Normal(0, fundamentalValueVariance);

            var fundamentalValue = new double[simulationSteps];

            fundamentalValue[0] = fundamentalValueInitial;

            for (int i = 1; i < simulationSteps; i++)
            {
                fundamentalValue[i] = CalculateNextValue(fundamentalValue[i - 1],standardNormal);
            }

            return fundamentalValue;
        }

        private static double CalculateNextValue(double currentValue, Normal normalDistribution)
        {
            return currentValue + normalDistribution.Sample();
        }
    }
}
