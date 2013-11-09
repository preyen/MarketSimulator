using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntradayTradingPatterns.TestParticipant.Console
{
    class UninformedAgent : Agent,IAgent
    {

        public UninformedAgent(Random randomNumberGenerator, int maxOrderQuantity, string name)
            : base(randomNumberGenerator, maxOrderQuantity, name)
        {
            TimingChromosome = new BitArray(3);

            for (int i = 0; i < TimingChromosome.Length; i++)
            {
                TimingChromosome[i] = randomNumberGenerator.Next() % 2 == 0;
            }
        }
        
        public override bool WillTradeInThisPeriod(int day, int tradingPeriod)
        {
            return tradingPeriod == getIntFromBitArray(TimingChromosome);
        }

        //http://stackoverflow.com/questions/5283180/how-i-can-convert-bitarray-to-single-int
        private int getIntFromBitArray(BitArray bitArray)
        {

            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];

        }

        public override void EvolveTimingChromosome(List<Agent> agents, double crossOverProbability, double mutationProbability)
        {
            if (_random.NextDouble() < crossOverProbability)
            {
                //selection
                var chromosomes =
                    agents.Select(a => new {a.TimingChromosome, Rank = a.CurrentProfit*_random.NextDouble()});
                
                var chosenChromosome = chromosomes.OrderBy(c => c.Rank).First();

                //crossover
                var crossOver = Math.Floor(_random.NextDouble() * TimingChromosome.Count);
                
                for (int i = 0; i < crossOver; i++)
                {
                    TimingChromosome[i] = chosenChromosome.TimingChromosome[i];
                }
            }

            //mutation
            for (int i = 0; i < TimingChromosome.Count; i++)
            {
                TimingChromosome[i] = _random.NextDouble() < mutationProbability
                    ? !TimingChromosome[i]
                    : TimingChromosome[i];
            }
        }
    }
}
