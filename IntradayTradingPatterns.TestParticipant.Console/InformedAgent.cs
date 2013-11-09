using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntradayTradingPatterns.TestParticipant.Console
{
    class InformedAgent : Agent,IAgent
    {
        bool _compete;
        

        public InformedAgent(Random randomNumberGenerator, int maxOrderQuantity, string name, bool compete)
            : base(randomNumberGenerator, maxOrderQuantity, name)
        {
            _compete = compete;

            TimingChromosome = new BitArray(8);

            for (int i = 0; i < TimingChromosome.Length; i++)
            {
                TimingChromosome[i] = randomNumberGenerator.Next() % 2 == 0;
            }
        }

        public override MarketSimulator.Contracts.Order FilterLimitOrders(MarketSimulator.Contracts.Order order)
        {
            if (!_compete)
            {
                return null;
            }
            else
            {
                return order;
            }
        }

        public override bool WillTradeInThisPeriod(int day, int tradingPeriod)
        {
            return TimingChromosome[tradingPeriod];
        }

        public override void EvolveTimingChromosome(List<Agent> agents, double crossOverProbability, double mutationProbability)
        {
            

        }
    }
}
