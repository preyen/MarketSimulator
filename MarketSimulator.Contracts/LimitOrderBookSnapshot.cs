using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSimulator.Contracts
{
    public class LimitOrderBookSnapshot
    {
        public double? BestAskPrice { get; set; }
        public double? BestBidPrice { get; set; }

        public double? BestAskQuantity { get; set; }
        public double? BestBidQuantity { get; set; }
    }
}
