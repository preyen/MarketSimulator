using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.CommunicationsModule;
using MarketSimulator.LimitOrderBook;

namespace MarketSimulator.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var limitOrderBook = new StandardLimitOrderBook();
            var commsModule = SignalRCommunicationsHandler.Instance;
            var simulator = new Simulator(limitOrderBook, commsModule, commsModule);

            System.Console.ReadKey();
        }
    }
}
