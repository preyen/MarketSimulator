using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Contracts;

namespace IntradayTradingPatterns.TestParticipant.Console
{
    interface IAgent
    {
        Order GetNextAction(LimitOrderBookSnapshot limitOrderBookSnapshot, int day, int tradingPeriod);
        List<string> GetOrdersToCancel(int day, int tradingPeriod);
    }
}
