using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Contracts;

namespace IntradayTradingPatterns.TestParticipant.Console
{
    public abstract class Agent : IAgent
    {
        public double ExpectedAssetLiquidationValue { get; set; }
        public double ExpectedAssetLiquidationValueOrderRange { get; set; }
        private Random _random;
        private int _maxOrderQuantity;
        private string _name;

        public Agent(Random randomNumberGenerator, int maxOrderQuantity, string name)
        {
            _random = randomNumberGenerator;
            _maxOrderQuantity = maxOrderQuantity;
            _name = name;
        }

        public MarketSimulator.Contracts.Order GetNextAction(MarketSimulator.Contracts.LimitOrderBookSnapshot limitOrderBookSnapshot, int day, int tradingPeriod)
        {
            if (!WillTradeInThisPeriod(day, tradingPeriod))
            {
                return null;
            }

            var randomDraw = (ExpectedAssetLiquidationValue - ExpectedAssetLiquidationValueOrderRange) + (_random.NextDouble() * ExpectedAssetLiquidationValueOrderRange * 2);

            var orderQuantity = Math.Round(_random.NextDouble() * _maxOrderQuantity,0);

            Order order = new Order();
            order.Price = randomDraw;
            order.Quantity = orderQuantity;
            order.UserID = _name;
            
            if (randomDraw > ExpectedAssetLiquidationValue)
            {
                //sell
                order.Side = OrderSide.Sell;
                if (randomDraw < limitOrderBookSnapshot.BestBidPrice)
                {
                    //sell market order
                    order.Type = OrderType.MarketOrder;
                }
                else
                {
                    //sell limit order
                    order.Type = OrderType.LimitOrder;
                }
            }
            else
            {
                //buy
                order.Side = OrderSide.Buy;
                if (randomDraw > limitOrderBookSnapshot.BestAskPrice)
                {
                    //buy market order
                    order.Type = OrderType.MarketOrder;
                }
                else
                {
                    //buy limit order
                    order.Type = OrderType.LimitOrder;
                }
            }

            return order;
        }

        public abstract bool WillTradeInThisPeriod(int day, int tradingPeriod);


        public List<string> GetOrdersToCancel(int day, int tradingPeriod)
        {
            throw new NotImplementedException();
        }
    }
}
