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
        BitArray timingChromosome;

        public UninformedAgent(Random randomNumberGenerator, int maxOrderQuantity, string name)
            : base(randomNumberGenerator, maxOrderQuantity, name)
        {
            timingChromosome = new BitArray(3);

            for (int i = 0; i < timingChromosome.Length; i++)
            {
                timingChromosome[i] = randomNumberGenerator.Next() % 2 == 0;
            }
        }
        
        public override bool WillTradeInThisPeriod(int day, int tradingPeriod)
        {
            return tradingPeriod == getIntFromBitArray(timingChromosome);
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

        public override void EvolveTimingChromosome()
        {
            
        }
    }
}
