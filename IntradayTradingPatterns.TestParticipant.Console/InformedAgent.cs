using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Contracts;

namespace IntradayTradingPatterns.TestParticipant.Console
{
    class InformedAgent : Agent,IAgent
    {
        bool _compete;
        

        public InformedAgent(Random randomNumberGenerator, int maxOrderQuantity, string name, bool compete, List<BitArray> allTimingChromosomes, double initialPropensity, double recency, double experimentation, double temperature, string @group)
            : base(randomNumberGenerator, maxOrderQuantity, name,allTimingChromosomes,initialPropensity,recency,experimentation,temperature,group)
        {
            _compete = compete;
        }

       

        public override MarketSimulator.Contracts.Order FilterLimitOrders(MarketSimulator.Contracts.Order order)
        {
            if (order != null && !_compete && order.Type == OrderType.LimitOrder)
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
