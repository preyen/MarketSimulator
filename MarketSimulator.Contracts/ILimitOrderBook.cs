using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSimulator.Contracts
{
    public interface ILimitOrderBook
    {
        SortedList<double, Queue<Order>> Bids {get;}
        SortedList<double, Queue<Order>> Asks { get; }

       Order BestBid { get; }
       Order BestAsk { get; }

      IEnumerable<OrderUpdate> ProcessLimitOrder(Order order);
      IEnumerable<OrderUpdate> AmendLimitOrder(Order order);

      IEnumerable<OrderUpdate> ProcessMarketOrder(Order order);

      IEnumerable<OrderUpdate> CancelOrder(Order order);        
    }
}
