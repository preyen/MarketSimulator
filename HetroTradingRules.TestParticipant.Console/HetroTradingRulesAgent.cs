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

        public HetroTradingRulesAgent(double sigma1, double sigma2, double sigmaN, double referenceAgentTimeHorizon, double referenceRiskAversionLevel)
        {
            var g1 = 1.0; //pure fundamentalist strategy
            var g2 = 1.0; //chartist strategy
            var n = 1.0; //noise trading strategy

            var agentTimeHorizon = Math.Floor(referenceAgentTimeHorizon * ((1 + g1)/(1+g2)));
            var agentRiskAversionLevel = referenceRiskAversionLevel * ((1 + g1) / (1 + g2));

        }

        public double GetAction(double spotPrice,double fundamentalValue, double noise) 
        {
            return 0;
        }

        public double GetPortfolioWealth(double spotPrice)
        {
            return (StockHeld * spotPrice) + CashHeld;
        }
    }
}
