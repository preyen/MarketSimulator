using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Contracts;

namespace IntradayTradingPatterns.TestParticipant.Console
{
    public abstract class Agent
    {        
        public double ExpectedAssetLiquidationValue { get; set; }
        public double ExpectedAssetLiquidationValueOrderRange { get; set; }
        public Random _random;
        private int _maxOrderQuantity;
        private string _name;

        public double CurrentProfit { get; set; }

        public string Name
        {
            get { return _name; }
        }

        BitArray timingChromosome;

        public BitArray TimingChromosome
        {
            get { return timingChromosome; }
            set { timingChromosome = value; }
        }


        public Agent(Random randomNumberGenerator, int maxOrderQuantity, string name)
        {
            _random = randomNumberGenerator;
            _maxOrderQuantity = maxOrderQuantity;
            _name = name;
            CurrentProfit = 0;
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
                if (limitOrderBookSnapshot != null && limitOrderBookSnapshot.BestBidPrice != null && randomDraw < limitOrderBookSnapshot.BestBidPrice)
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
                if (limitOrderBookSnapshot != null && limitOrderBookSnapshot.BestAskPrice != null && randomDraw > limitOrderBookSnapshot.BestAskPrice)
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

            order = FilterLimitOrders(order);

            return order;
        }        

        public virtual Order FilterLimitOrders(Order order)
        {
            return order; 
        }

        public abstract bool WillTradeInThisPeriod(int day, int tradingPeriod);


        public List<string> GetOrdersToCancel(int day, int tradingPeriod)
        {
            return new List<string>();
        }
               

        public abstract void EvolveTimingChromosome(List<Agent> agents, double crossOverProbability,double mutationProbability);
        
    }
}
