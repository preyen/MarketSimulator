using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Contracts;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;

namespace MarketSimulator.CommunicationsModule
{
    public class SignalRCommunicationsHandler : IDataCommunicationsModule, IOrderCommunicationsModule
    {
        public SignalRCommunicationsHandler(string hostURL)
        {
            using (WebApplication.Start<Startup>(hostURL))
            {
                Console.WriteLine("Server running on {0}", hostURL);
                Console.ReadLine();
            }

        }
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

    class Startup
    {
        public void Configuration(IAppBuilder app)
        {            
            app.MapHubs();
        }
    }

    public class MyHub : Hub
    {
        public void Send(ILimitOrderBook limitOrderBook)
        {
            Clients.All.UpdateOrderBook(limitOrderBook);
        }
    }


}
