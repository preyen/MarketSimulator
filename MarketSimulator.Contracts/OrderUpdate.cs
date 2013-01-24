using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSimulator.Contracts
{
    public class OrderUpdate
    {
        public Order Order { get; set; }

        public Boolean Placed { get; set; }
        public Boolean Canceled { get; set; }
        public Boolean Amended { get; set; }

        public string Message { get; set; }

       public List<Match> Matches { get; set; }
    }
}
