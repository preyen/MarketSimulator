using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSimulator.Contracts
{
    public class Order
    {        
        public string ID { get; private set; }
        public string UserID { get;  set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public double Quantity { get; set; }
        public double Price { get; set; }
        public double? StopPrice { get; set; }
        public OrderExecutionValidity ExecutionValidity { get; set; }
        public OrderTimeValidity TimeValidity { get; set; }
        public DateTime? ValidUntil { get; set; }
        public Boolean Valid { get; set; }

        public Order()
        {
            ID = Guid.NewGuid().ToString();
            Valid = true;
        }
    }
}
