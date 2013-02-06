using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketSimulator.Contracts;

namespace MarketSimulator
{
    public class Simulator
    {
        private ILimitOrderBook _limitOrderBook;
        private IDataCommunicationsModule _dataCommunicationsModule;
        private IOrderCommunicationsModule _orderCommunicationsModule;

        public Simulator(ILimitOrderBook limitOrderBook, IDataCommunicationsModule dataCommunicationsModule, IOrderCommunicationsModule orderCommunicationsModule)
        {
            _limitOrderBook = limitOrderBook;
            _dataCommunicationsModule = dataCommunicationsModule;
            _orderCommunicationsModule = orderCommunicationsModule;

            _orderCommunicationsModule.OnOrder += _orderCommunicationsModule_OnOrder;
        }

        void _orderCommunicationsModule_OnOrder(Order order, string userID)
        {
            IEnumerable<OrderUpdate> orderUpdates = null;

            switch (order.Type)
            {
                case OrderType.LimitOrder:
                    orderUpdates =_limitOrderBook.ProcessLimitOrder(order);
                    break;
                case OrderType.MarketOrder:
                    orderUpdates = _limitOrderBook.ProcessMarketOrder(order);
                    break;
                case OrderType.StopLimitOrder:
                    break;
                case OrderType.StopMarketOrder:
                    break;
                default:
                    throw new Exception("Unknown order type");
            }

            if (orderUpdates != null)
            {
                foreach (var orderUpdate in orderUpdates)
                {
                    _orderCommunicationsModule.PushOrderInstructionUpdate(orderUpdate, orderUpdate.Order.UserID);
                }
            }

            _dataCommunicationsModule.PushUpdate(_limitOrderBook);
        }
    }
}
