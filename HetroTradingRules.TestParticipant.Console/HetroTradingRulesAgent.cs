using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public HetroTradingRulesAgent(double fundementalistWeighting, double chartistWeighting, double noiseWeighting, double referenceAgentTimeHorizon, double referenceRiskAversionLevel)
        {
            _fundamentalistWeighting = fundementalistWeighting;
            _chartistWeighting = chartistWeighting;
            _noiseWeighting = noiseWeighting;
            _agentTimeHorizon = (int) Math.Floor(referenceAgentTimeHorizon * ((1 + fundementalistWeighting) / (1 + chartistWeighting)));
            _agentRiskAversionLevel = referenceRiskAversionLevel * ((1 + fundementalistWeighting) / (1 + chartistWeighting));         
        }

        public double GetAction(int timeStep, double[] spotPrice,double[] fundamentalValue, double[] noise) 
        {
            var fundamentalTimeHorizon = _agentTimeHorizon;

            var normalisationTerm = 1/(_fundamentalistWeighting + _chartistWeighting + _noiseWeighting);

            var fundamentalistTerm = _fundamentalistWeighting * (1/fundamentalTimeHorizon) * Math.Log(fundamentalValue[timeStep]/spotPrice[timeStep]);

            var averageReturn = (1 / _agentTimeHorizon) * Enumerable.Range(1,_agentTimeHorizon).Aggregate(0d,(curr,j) => curr + Math.Log(spotPrice[timeStep-j] / spotPrice[timeStep-j-1]));
            var chartistTerm = _chartistWeighting * averageReturn;

            var noiseTerm = _noiseWeighting * noise[timeStep];


            var expectedReturn = normalisationTerm * (fundamentalistTerm + chartistTerm + noiseTerm);

            var expectedPrice = spotPrice[timeStep] * Math.Exp(expectedReturn * _agentTimeHorizon);



            return 0;
        }

        public double GetPortfolioWealth(double spotPrice)
        {
            return (StockHeld * spotPrice) + CashHeld;
        }
    }
}

