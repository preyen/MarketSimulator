using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketSimulator.Utils;
using MarketSimulator.Contracts;

namespace MarketSimulator.Agents
{
    public class RandomLiquidityMaker : IAgent
    {
        private IRandomNumberGenerator _rng;
        private double _maxOrderSize;
        private double _maxPriceDifferential;
        private double _doNothingProbability;        

        public RandomLiquidityMaker(IRandomNumberGenerator randomNumberGenerator, double maxOrderSize, double maxPriceDifferential, double doNothingProbability)
        {
            _rng = randomNumberGenerator;
            _maxOrderSize = maxOrderSize;
            _maxPriceDifferential = maxPriceDifferential;
            _doNothingProbability = doNothingProbability;            
        }
        public Order GetNextAction(LimitOrderBookSnapshot limitOrderBook)
        {            
            var size = _maxOrderSize * _rng.GetRandomDouble();
            var priceDiff = ((limitOrderBook.BestBidPrice.Value + limitOrderBook.BestAskPrice.Value) / 2)*_maxPriceDifferential * _rng.GetRandomDouble();

            Order order;

            var randomNumber = _rng.GetRandomDouble();
            if (randomNumber > (0.5 + (_doNothingProbability/2)))
            {
                //buylimitorder
                var price = limitOrderBook.BestAskPrice.Value - priceDiff;
                order = new Order { Price = price, Quantity = size, Side = OrderSide.Buy,Type = OrderType.LimitOrder};
                
            }
            else if (randomNumber < (0.5 - (_doNothingProbability/2)))
            {
                //selllimitorder
                var price = limitOrderBook.BestBidPrice.Value + priceDiff;
                order = new Order { Price = price, Quantity = size, Side = OrderSide.Sell,Type = OrderType.LimitOrder };
            }
            else
            {
                //donothing
                order = null;
            }

            return order;
        }
    }
}
