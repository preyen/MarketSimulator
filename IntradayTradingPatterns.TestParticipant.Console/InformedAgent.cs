using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntradayTradingPatterns.TestParticipant.Console
{
    class InformedAgent : Agent,IAgent
    {
        public InformedAgent(Random randomNumberGenerator, int maxOrderQuantity, string name)
            : base(randomNumberGenerator, maxOrderQuantity, name)
        {
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
