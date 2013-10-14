using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntradayTradingPatterns.TestParticipant.Console
{
    class UninformedAgent : Agent,IAgent
    {
        public UninformedAgent(Random randomNumberGenerator, int maxOrderQuantity, string name)
            : base(randomNumberGenerator, maxOrderQuantity, name)
        {
        }
        
        public override bool WillTradeInThisPeriod(int day, int tradingPeriod)
        {
            throw new NotImplementedException();
        }
    }
}
