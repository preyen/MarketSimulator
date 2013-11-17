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

        public UninformedAgent(Random randomNumberGenerator, int maxOrderQuantity, string name,
            List<BitArray> allTimingChromosomes, double initialPropensity, double recency, double experimentation,
            double temperature)
            : base(
                randomNumberGenerator, maxOrderQuantity, name, allTimingChromosomes, initialPropensity, recency,
                experimentation, temperature)
        {

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
    }
}
