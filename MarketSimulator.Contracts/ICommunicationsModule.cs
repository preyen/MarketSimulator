using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarketSimulator.Contracts
{
    public delegate void ProcessOrderHandler(Order order, string userID);

    public interface IDataCommunicationsModule
    {
       bool SubscribeToDataFeed(string userID);
       bool UnsubscribeFromDataFeed(string userID);
       bool PushUpdate(ILimitOrderBook limitOrderBook);        
    }

    public interface IOrderCommunicationsModule
    {      
       event ProcessOrderHandler OnOrder;

       bool ProcessOrderInstruction(Order order, string userID);
       bool PushOrderInstructionUpdate(OrderUpdate order, string userID);
    }
}
