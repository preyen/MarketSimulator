using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSimulator.Contracts
{
    public interface IDataCommunicationsModule
    {
       bool SubscribeToDataFeed(string userID);
       bool PushUpdate(ILimitOrderBook limitOrderBook);        
    }

    public interface IOrderCommunicationsModule
    {
       bool ProcessOrderInstruction(Order order, string userID);
       bool PushOrderInstructionUpdate(OrderUpdate order, string userID);
    }
}
