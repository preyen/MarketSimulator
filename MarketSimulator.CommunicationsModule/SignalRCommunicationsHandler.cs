using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketSimulator.Contracts;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Hosting;
using Owin;

namespace MarketSimulator.CommunicationsModule
{
    public class SignalRCommunicationsHandler : IDataCommunicationsModule, IOrderCommunicationsModule
    {
        private readonly static Lazy<SignalRCommunicationsHandler> _instance = new Lazy<SignalRCommunicationsHandler>(() => new SignalRCommunicationsHandler());
        private readonly Lazy<IHubConnectionContext> _clientsInstance = new Lazy<IHubConnectionContext>(() => GlobalHost.ConnectionManager.GetHubContext<MarketCommunications>().Clients);

        public static SignalRCommunicationsHandler Instance {get {return _instance.Value;}}

        public ConcurrentDictionary<string,string> ConnectionIDs = new ConcurrentDictionary<string,string>();
        public List<string> DataListeners = new List<string>();

        public SignalRCommunicationsHandler()
        {
            using (var wa = WebApplication.Start<Startup>(@"http://Preyen-PC:8088"))
            {
                Console.WriteLine("Server running on {0}", "unknown");
               
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
            var dataConnections = ConnectionIDs.Where(c => DataListeners.Contains(c.Key)).Select(c => c.Value);

            foreach (var connID in dataConnections)
            {
                _clientsInstance.Value.Client(connID).Update(limitOrderBook);
            }

            return true;
        }

        public event ProcessOrderHandler OnOrder;

        public bool ProcessOrderInstruction(Order order, string userID)
        {
            OnOrder(order, userID);
            return true;
        }

        public bool PushOrderInstructionUpdate(OrderUpdate order, string userID)
        {
            var connID = ConnectionIDs[userID];
            _clientsInstance.Value.Client(connID).Update(order);
            

            return true;
        }
    }

    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapHubs();
        }
    }

    public class MarketCommunications : Hub
    {
        SignalRCommunicationsHandler _commsHandler;

        public MarketCommunications() : this(SignalRCommunicationsHandler.Instance) { }

        public MarketCommunications(SignalRCommunicationsHandler commsHandler)
        {
            _commsHandler = commsHandler;
        }
        public bool ProcessOrderInstruction(Order order, string userID)
        {
            RegisterUserID(userID, Clients.Caller.ConnectionID);
            return _commsHandler.ProcessOrderInstruction(order,userID);
        }
        public bool SubscribeToDataFeed(string userID)
        {
            RegisterUserID(userID, Clients.Caller.ConnectionID);
            if (!_commsHandler.DataListeners.Contains(userID))
            _commsHandler.DataListeners.Add(userID);
            return true;
        }

        public bool UnsubscribeFromDataFeed(string userID)
        {
            if (_commsHandler.DataListeners.Contains(userID))
                _commsHandler.DataListeners.Remove(userID);
            return true;
        }

        private void RegisterUserID(string userID, string connectionID) 
        {
            _commsHandler.ConnectionIDs.AddOrUpdate(userID,connectionID,(oldValue,newValue) => newValue);
        }

        public void Ping()
        {
            Clients.Caller.Pong("hello!");
        }
    }


}
