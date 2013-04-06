using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Agents;
using MarketSimulator.Contracts;
using MarketSimulator.Utils;
using MathNet.Numerics.Distributions;

namespace RealTime.TestParticipant.Console
{
    public class LiquidityProvider : RandomLiquidityMaker
    {
        Order _lastOrder;

        public LiquidityProvider(IRandomNumberGenerator randomNumberGenerator, double maxOrderSize, double maxPriceDifferential, double doNothingProbability, Normal normalDist, int decimals)
            : base(randomNumberGenerator, maxOrderSize, maxPriceDifferential, doNothingProbability, normalDist, decimals)
        {
        }

        public Order[] GetNextActions(LimitOrderBookSnapshot limitOrderBook)
        {
            var order = base.GetNextAction(limitOrderBook);
            
            var orders = new List<Order>();

            if (order != null)
            {
                if (_lastOrder != null)
                {
                    //cancel previous
                    var cancelOrder = new Order();
                    cancelOrder.Type = OrderType.Cancel;
                    cancelOrder.ID = _lastOrder.ID;

                    orders.Add(cancelOrder);
                }

                orders.Add(order);

                _lastOrder = order;
            }

            return orders.ToArray();
        }
    }
}
