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
        private readonly double _initialPropensity;
        private readonly double _recency;
        private readonly double _experimentation;
        private readonly double _temperature;
        private readonly string _group;

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

        public string Group
        {
            get { return _group; }
        }

        private List<ChromosomeLearning> _learningLog;

        public Agent(Random randomNumberGenerator, int maxOrderQuantity, string name,IReadOnlyCollection<BitArray> allTimingChromosomes, double initialPropensity, double recency, double experimentation,
            double temperature, string group)
        {
            _random = randomNumberGenerator;
            _maxOrderQuantity = maxOrderQuantity;
            _name = name;
            _initialPropensity = initialPropensity;
            _recency = recency;
            _experimentation = experimentation;
            _temperature = temperature;
            _group = @group;

            CurrentProfit = 0;

            _learningLog = new List<ChromosomeLearning>();

            foreach (var chromosome in allTimingChromosomes)
            {
                _learningLog.Add(new ChromosomeLearning()
                {
                    Probability = 1d/allTimingChromosomes.Count,
                    Propensity = initialPropensity,
                    TimingChromosome = chromosome
                });
            }

            TimingChromosome = _learningLog.OrderBy(l => l.Probability*_random.NextDouble()).First().TimingChromosome;
            
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

        public void RandomizeTimingChromosome(Random randomNumberGenerator)
        {
            for (int i = 0; i < TimingChromosome.Length; i++)
            {
                TimingChromosome[i] = randomNumberGenerator.Next() % 2 == 0;
            }
        }


        public virtual void EvolveTimingChromosome(LearningMode learningMode, Dictionary<BitArray,double> agents,
            double crossOverProbability, double mutationProbability)
        {
            switch (learningMode)
            {
                case LearningMode.Random:
                    RandomizeTimingChromosome(_random);
                    break;
                case LearningMode.GA:
                    if (_random.NextDouble() < crossOverProbability)
                    {
                        //selection
                        var chromosomes =
                            agents.Select(a => new {a.Key, Rank = a.Value*_random.NextDouble()});

                        var chosenChromosome = chromosomes.OrderByDescending(c => c.Rank).First();

                        //crossover

                        var crossOver = Math.Floor(_random.NextDouble()*TimingChromosome.Count);

                        for (int i = 0; i < crossOver; i++)
                        {
                            TimingChromosome[i] = chosenChromosome.Key[i];
                        }
                    }

                    //mutation
                    for (int i = 0; i < TimingChromosome.Count; i++)
                    {
                        TimingChromosome[i] = _random.NextDouble() < mutationProbability
                            ? !TimingChromosome[i]
                            : TimingChromosome[i];
                    }
                    break;

                case LearningMode.MRE:
                    //update propensities
                    for (int i = 0; i < _learningLog.Count; i++)
                    {
                        double reward;

                        if (_learningLog[i].TimingChromosome == TimingChromosome)
                        {
                            reward = CurrentProfit*(1 - _experimentation);
                        }
                        else
                        {
                            reward = _learningLog[i].Propensity * (_experimentation / (_learningLog.Count -1));
                        }

                        _learningLog[i].Propensity = (1 - _recency)*_learningLog[i].Propensity + reward;
                    }
                
                    //update probabilities
                    for (int i = 0; i < _learningLog.Count; i++)
                    {
                        _learningLog[i].Probability = Math.Exp(_learningLog[i].Propensity/_temperature)/
                                                      _learningLog.Sum(l => Math.Exp(l.Propensity/_temperature));
                    }

                    //select chromosome
                    TimingChromosome = _learningLog.OrderByDescending(l => l.Probability * _random.NextDouble()).First().TimingChromosome;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("Unknown learning mode");
            }

            CurrentProfit = 0;
        }
        
    }

    public class ChromosomeLearning
    {
        public BitArray TimingChromosome { get; set; }
        public double Propensity { get; set; }
        public double Probability { get; set; }
    }

    public enum LearningMode
    {
        Random,
        GA,
        MRE
    }
}
