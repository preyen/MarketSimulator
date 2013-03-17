using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketSimulator.Contracts;

namespace MarketSimulator.Agents {

    public interface IAgent
    {
        Order GetNextAction(LimitOrderBookSnapshot limitOrderBook);
    }
}
