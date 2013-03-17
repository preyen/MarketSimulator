using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSimulator.Utils
{
    public class CSharpRandomNumberGenerator : IRandomNumberGenerator
    {
        Random _rng;

        public CSharpRandomNumberGenerator()
        {
            _rng = new Random();
        }
        public double GetRandomDouble()
        {
            return _rng.NextDouble();
        }
    }
}
