using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketSimulator.Contracts;
using MarketSimulator.Utils;

namespace MarketSimulator.Agents
{
    public class RandomLiquidityTaker : IAgent
    {
        IRandomNumberGenerator _rng;
        double _doNothingProbability;
        double _maxOrderSize;        

        public RandomLiquidityTaker(IRandomNumberGenerator randomNumberGenerator, double maxOrderSize, double doNothingProbability)
        {
            _rng = randomNumberGenerator;
            _doNothingProbability = doNothingProbability;
            _maxOrderSize = maxOrderSize;            
        }
        public Order GetNextAction(LimitOrderBookSnapshot limitOrderBook)
        {
            Order order;
            var size = Math.Ceiling(_maxOrderSize * _rng.GetRandomDouble());

            var randomNumber = _rng.GetRandomDouble();

            if (randomNumber < (0.5 - (_doNothingProbability / 2)))
            {
                //buymarketdorder
                order = new Order() {Quantity = size,Type = OrderType.MarketOrder, Side = OrderSide.Buy };
            }
            else if (randomNumber > (0.5 + (_doNothingProbability / 2)))
            {
                order = new Order() { Quantity = size, Type = OrderType.MarketOrder, Side = OrderSide.Sell };
            }
            else
            {
                //do nothing
                order = null;
            }

            return order;
        }        
    }
}
