using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketSimulator.Utils;
using MarketSimulator.Contracts;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace MarketSimulator.Agents
{
    public class RandomLiquidityMaker : IAgent
    {
        private IRandomNumberGenerator _rng;
        private double _maxOrderSize;
        private double _maxPriceDifferential;
        private double _doNothingProbability;
        private Normal _normal;
        private int _decimals;

        public RandomLiquidityMaker(IRandomNumberGenerator randomNumberGenerator, double maxOrderSize, double maxPriceDifferential, double doNothingProbability, Normal normalDist,int decimals)
        {
            _rng = randomNumberGenerator;
            _maxOrderSize = maxOrderSize;
            _maxPriceDifferential = maxPriceDifferential;
            _doNothingProbability = doNothingProbability;
            _normal = normalDist;
            _decimals = decimals;
        }
        public virtual Order GetNextAction(LimitOrderBookSnapshot limitOrderBook)
        {            
            var size = Math.Ceiling(_maxOrderSize * _rng.GetRandomDouble());
            var priceDiff = _maxPriceDifferential * Math.Abs(_normal.Sample()); //((limitOrderBook.BestBidPrice.Value + limitOrderBook.BestAskPrice.Value) / 2) * _maxPriceDifferential * Math.Abs(_normal.Sample());// _rng.GetRandomDouble();

            Order order;

            var randomNumber = _rng.GetRandomDouble();
            if (randomNumber > (0.5 + (_doNothingProbability/2)))
            {
                //buylimitorder
                var price = Math.Round(limitOrderBook.BestAskPrice.Value - priceDiff, _decimals);
                order = new Order { Price = price, Quantity = size, Side = OrderSide.Buy,Type = OrderType.LimitOrder, Valid = true};
                
            }
            else if (randomNumber < (0.5 - (_doNothingProbability/2)))
            {
                //selllimitorder
                var price = Math.Round(limitOrderBook.BestBidPrice.Value + priceDiff, _decimals);
                order = new Order { Price = price, Quantity = size, Side = OrderSide.Sell, Type = OrderType.LimitOrder, Valid = true };
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
