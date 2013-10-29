using System;
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
            return base._random.Next() % 2 == 0;
        }

        public override void EvolveTimingChromosome()
        {
            
        }
    }
}
