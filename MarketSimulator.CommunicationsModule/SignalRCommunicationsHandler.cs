using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Contracts;

namespace MarketSimulator.CommunicationsModule
{
    public class SignalRCommunicationsHandler : IDataCommunicationsModule, IOrderCommunicationsModule
    {        
        public bool SubscribeToDataFeed(string userID)
        {
            throw new NotImplementedException();
        }

        public bool UnsubscribeFromDataFeed(string userID)
        {
            throw new NotImplementedException();
        }

        public bool PushUpdate(ILimitOrderBook limitOrderBook)
        {
            throw new NotImplementedException();
        }

        public event ProcessOrderHandler OnOrder;

        public bool ProcessOrderInstruction(Order order, string userID)
        {
            throw new NotImplementedException();
        }

        public bool PushOrderInstructionUpdate(OrderUpdate order, string userID)
        {
            throw new NotImplementedException();
        }
    }
}
