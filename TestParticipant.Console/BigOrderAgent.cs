using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Agents;

namespace TestParticipant.Console
{
    public class BigOrderAgent : IAgent
    {
        int count = 0;

        public MarketSimulator.Contracts.Order GetNextAction(MarketSimulator.Contracts.LimitOrderBookSnapshot limitOrderBook)
        {
            count++;

            if (count == 50)
            {
                var order = new MarketSimulator.Contracts.Order();
                order.Valid = true;
                order.Type = MarketSimulator.Contracts.OrderType.MarketOrder;
                order.Quantity = 10000;
                order.Side = MarketSimulator.Contracts.OrderSide.Buy;
                order.UserID = "Whale";

                return order;

            }

                return null;
        }
    }
}
