using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSimulator.Contracts
{
    public enum OrderType
    {
        LimitOrder,
        MarketOrder,
        StopLimitOrder,
        StopMarketOrder,
        Cancel,
        Amend
    }
}
