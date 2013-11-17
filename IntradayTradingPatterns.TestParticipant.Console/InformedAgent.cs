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
        

        public InformedAgent(Random randomNumberGenerator, int maxOrderQuantity, string name, bool compete, List<BitArray> allTimingChromosomes, double initialPropensity, double recency, double experimentation, double temperature)
            : base(randomNumberGenerator, maxOrderQuantity, name,allTimingChromosomes,initialPropensity,recency,experimentation,temperature)
        {
            _compete = compete;
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
    }
}
