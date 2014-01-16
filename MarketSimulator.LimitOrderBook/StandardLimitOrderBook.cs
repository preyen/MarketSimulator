using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarketSimulator.Contracts;

namespace MarketSimulator.LimitOrderBook
{
    public class StandardLimitOrderBook : ILimitOrderBook
    {
        public StandardLimitOrderBook()
        {
            Bids = new SortedList<double, Queue<Order>>();
            Asks = new SortedList<double, Queue<Order>>();
            StopAsks = new SortedList<double, Queue<Order>>();
            StopBids = new SortedList<double, Queue<Order>>();
        }

        public SortedList<double, Queue<Order>> Bids
        {
            get;
            private set;
        }

        public SortedList<double, Queue<Order>> Asks
        {
            get;
            private set;
        }

        public SortedList<double,Queue<Order>> StopAsks
        {
            get;
            private set;
        }
        public SortedList<double, Queue<Order>> StopBids
        {
            get;
            private set;
        }

        public Order BestBid
        {
            get { return Bids.Count != 0 ? new Order { Price = Bids.Last().Key, Quantity = Bids.Last().Value.Sum(b => b.Quantity) } : null; }
        }

        public Order BestAsk
        {
            get { return Asks.Count != 0 ? new Order { Price = Asks.First().Key, Quantity = Asks.First().Value.Sum(a => a.Quantity)} : null
; }
        }

        public void ClearAllOrders()
        {
            Bids.Clear();
            Asks.Clear();
        }

        public IEnumerable<OrderUpdate> ProcessLimitOrder(Order order,bool checkStopOrders)
        {
            var results = new List<OrderUpdate>();

            switch (order.Side)
            {
                case OrderSide.Buy:
                    results.AddRange(ProcessBuyLimitOrder(order));
                    break;
                case OrderSide.Sell:
                    results.AddRange(ProcessSellLimitOrder(order));
                    break;
                default:
                    results.Add(new OrderUpdate()
                    {
                        Placed = false,
                        Order = order,
                        Message = "Order side not known"
                    });
                    break;
            }

            if (checkStopOrders)
                results.AddRange(ClearStopOrders());

            return results;
        }

        private IEnumerable<OrderUpdate> ClearStopOrders()
        {
            var results = new List<OrderUpdate>();

            var bidsToRemove = new List<double>();
            var asksToRemove = new List<double>();

            //clear asks (sells)
            if (BestBid != null && StopAsks.Where(a => a.Key <= BestBid.Price).Any())
            {
                foreach (var stopAskQueue in StopAsks)
                {
                    if (stopAskQueue.Key <= BestBid.Price)
                    {
                        foreach (var stopAsk in stopAskQueue.Value)
                        {
                            switch (stopAsk.Type)
                            {                                
                                case OrderType.StopLimitOrder:
                                    results.AddRange(ProcessLimitOrder(stopAsk,true));
                                    break;
                                case OrderType.StopMarketOrder:
                                    results.AddRange(ProcessMarketOrder(stopAsk,true));
                                    break;                             
                                default:
                                    break;
                            }
                        }
                        asksToRemove.Add(stopAskQueue.Key);
                    }
                }
            }

            //clear bids (buys)
            if (BestAsk != null && StopBids.Where(a => a.Key >= BestAsk.Price).Any())
            {
                foreach (var stopBidQueue in StopBids)
                {
                    if (stopBidQueue.Key >= BestAsk.Price)
                    {
                        foreach (var stopBid in stopBidQueue.Value)
                        {
                            switch (stopBid.Type)
                            {
                                case OrderType.StopLimitOrder:
                                    results.AddRange(ProcessLimitOrder(stopBid,true));
                                    break;
                                case OrderType.StopMarketOrder:
                                    results.AddRange(ProcessMarketOrder(stopBid,true));
                                    break;
                                default:
                                    break;
                            }
                        }
                        bidsToRemove.Add(stopBidQueue.Key);
                    }
                }
            }

            foreach (var ask in asksToRemove)
            {
                StopAsks.Remove(ask);
            }

            foreach (var bid in bidsToRemove)
            {
                StopBids.Remove(bid);
            }

            return results;
        }

        private IEnumerable<OrderUpdate> ProcessSellLimitOrder(Order order)
        {
            if (Bids.Any() && order.Price < Bids.Last().Key)
            {
                //cross order
                return new [] {new OrderUpdate() { Message = "Cross order", Order = order }};
            }
            if (!Asks.Keys.Contains(order.Price))
            {
                Asks.Add(order.Price, new Queue<Order>());
            }

            Asks[order.Price].Enqueue(order);

            return new[] {new OrderUpdate()
            {
                Placed = true,
                Order = order
            }};
        }

        private IEnumerable<OrderUpdate> ProcessBuyLimitOrder(Order order)
        {
            if (Asks.Any() && order.Price > Asks.First().Key)
            {
                //cross order
                return new[] { new OrderUpdate() { Message = "Cross order", Order = order } };
            }
            if (!Bids.Keys.Contains(order.Price))
            {
                Bids.Add(order.Price, new Queue<Order>());
            }

            Bids[order.Price].Enqueue(order);

            return new[] {new OrderUpdate()
            {
                Placed = true,
                Order = order
            }};
        }

        public IEnumerable<OrderUpdate> AmendLimitOrder(Order order)
        {
            return AmendLimitOrder(order, false);
        }

        public IEnumerable<OrderUpdate> AmendLimitOrder(Order order,bool cancel)
        {
            var amended = false;
            var remove = false;

            foreach (var bidGroup in Bids.Values)
            {
                foreach (var bid in bidGroup)
                {
                    if (bid.ID == order.ID)
                    {
                        if (cancel)
                        {
                            bid.Valid = false;

                            if (bidGroup.Count() <= 1)
                            {
                                remove = true;
                                Bids.Remove(bid.Price);
                            }
                            
                            return new[] {new OrderUpdate() {
                            Amended = false,Message = "Order Canceled",Order = bid,Placed=false}
                            };
                        }

                        if (bid.Price != order.Price)
                        {
                            bid.Valid = false;

                            if (bidGroup.Count() <= 1)
                            {
                                remove = true;
                                Bids.Remove(bid.Price);
                            }

                            return ProcessLimitOrder(order,true);
                        }
                        else
                        {
                            bid.Quantity = order.Quantity;
                        }
                        return new[] {new OrderUpdate() {
                            Amended = true,Message = "Order amended",Order = bid,Placed=true}
                        };                        
                    }
                }
            }

            foreach (var askGroup in Asks.Values)
            {
                foreach (var ask in askGroup)
                {
                    if (ask.ID == order.ID)
                    {
                        if (cancel)
                        {
                            ask.Valid = false;

                            if (askGroup.Count() <= 1)
                            {
                                remove = true;
                                Asks.Remove(ask.Price);
                            }

                            return new[] {new OrderUpdate() {
                            Amended = false,Message = "Order Canceled",Order = ask,Placed=false}
                            };
                        }

                        if (ask.Price != order.Price)
                        {
                            ask.Valid = false;
                            if (askGroup.Count() <= 1)
                            {
                                remove = true;
                                Asks.Remove(ask.Price);
                            }
                            return ProcessLimitOrder(order,true);
                        }
                        else
                        {
                            ask.Quantity = order.Quantity;
                        }
                        return new[] {new OrderUpdate() {
                            Amended = true,Message = "Order amended",Order = ask,Placed=true}
                        };                                                
                    }
                }
            }           

            return new[] {new OrderUpdate() {
                Message = "Order not found",
                Order = order                
            }};
        }

        public IEnumerable<OrderUpdate> ProcessMarketOrder(Order order, bool checkStopOrders)
        {
            var results = new List<OrderUpdate>();

            switch (order.Side)
            {
                case OrderSide.Buy:
                    results.AddRange(ProcessBuyMarketOrder(order));
                    break;
                case OrderSide.Sell:
                    results.AddRange(ProcessSellMarketOrder(order));
                    break;
                default:
                    results.Add(new OrderUpdate()
                    {
                        Placed = false,
                        Order = order,
                        Message = "Order side not known"
                    });
                    break;
            }

            if (checkStopOrders)
                results.AddRange(ClearStopOrders());

            return results;
        }

        private IEnumerable<OrderUpdate> ProcessSellMarketOrder(Order order)
        {
            if (order.ExecutionValidity == OrderExecutionValidity.ForceOrKill)
            {
                if (order.Quantity > Bids.Sum(a => a.Value.Sum(l => l.Quantity)))
                {
                    return new[] {new OrderUpdate()
                    {
                        Placed = false,
                        Order = order,
                        Message = "FOK validity and not enough depth"
                    }};
                }
            }

            var quantity = order.Quantity;

            var matches = new List<Match>();
            var orderUpdates = new List<OrderUpdate>();

            while (quantity > 0 && Bids.Count != 0)
            {
                var prices = Bids.Keys;
                for (int i = prices.Count - 1; i >= 0; i--)
                {
                    var price = prices[i];
                    var orders = Bids[price];

                    while (orders.Any() && quantity > 0)
                    {
                        var nextOrder = orders.Peek();
                        if (!nextOrder.Valid)
                        {
                            orders.Dequeue();
                            continue;
                        }

                        if (nextOrder.Quantity > quantity)
                        {
                            nextOrder.Quantity -= quantity;
                            matches.Add(new Match() { Price = price, Quantity = quantity });
                            orderUpdates.Add(new OrderUpdate()
                            {
                                Order = nextOrder,
                                Matches = new List<Match> { new Match() { Price = price, Quantity = quantity } }
                            });
                            quantity = 0;
                        }
                        if (nextOrder.Quantity <= quantity)
                        {
                            quantity -= nextOrder.Quantity;
                            matches.Add(new Match() { Price = price, Quantity = nextOrder.Quantity });
                            orderUpdates.Add(new OrderUpdate()
                            {
                                Order = nextOrder,
                                Matches = new List<Match> { new Match() { Price = price, Quantity = nextOrder.Quantity } }
                            });
                            orders.Dequeue();
                        }
                    }

                    if (!orders.Any()) Bids.Remove(price);
                }
            }

            orderUpdates.Add(new OrderUpdate()
            {
                Placed = true,
                Order = order,
                Matches = matches
            });


            return orderUpdates;
        }

        private IEnumerable<OrderUpdate> ProcessBuyMarketOrder(Order order)
        {
            if (order.ExecutionValidity == OrderExecutionValidity.ForceOrKill)
            {
                if (order.Quantity > Asks.Sum(a => a.Value.Sum(l => l.Quantity)))
                {
                    return new[] {new OrderUpdate()
                    {
                        Placed = false,
                        Order = order,
                        Message = "FOK validity and not enough depth"
                    }};
                }
            }

            var quantity = order.Quantity;

            var matches = new List<Match>();
            var orderUpdates = new List<OrderUpdate>();

            while (quantity > 0 && Asks.Count != 0)
            {
                var prices = Asks.Keys;
                for (int i = 0; i < prices.Count; i++)
                {
                    var price = prices[i];
                    var orders = Asks[price];

                    while (orders.Any() && quantity > 0)
                    {
                        var nextOrder = orders.Peek();
                        if (!nextOrder.Valid)
                        {
                            orders.Dequeue();
                            continue;
                        }

                        if (nextOrder.Quantity > quantity)
                        {
                            nextOrder.Quantity -= quantity;
                            matches.Add(new Match() { Price = price, Quantity = quantity });
                            orderUpdates.Add(new OrderUpdate()
                            {
                                Order = nextOrder,
                                Matches = new List<Match> { new Match() { Price = price, Quantity = quantity} }
                            });                            
                            quantity = 0;
                        }
                        if (nextOrder.Quantity <= quantity)
                        {
                            quantity -= nextOrder.Quantity;
                            matches.Add(new Match() { Price = price, Quantity = nextOrder.Quantity });
                            orderUpdates.Add(new OrderUpdate()
                            {
                                Order = nextOrder,
                                Matches = new List<Match> { new Match() { Price = price, Quantity = nextOrder.Quantity } }
                            }); 
                            orders.Dequeue();
                        }

                        if (!orders.Any()) Asks.Remove(price);
                    }
                }
            }

            orderUpdates.Add(new OrderUpdate()
            {
                Placed = true,
                Order = order,
                Matches = matches
            });


            return orderUpdates;
        }

        public IEnumerable<OrderUpdate> CancelOrder(Order order)
        {
            return AmendLimitOrder(order, true);            
        }


        public IEnumerable<OrderUpdate> ProcessStopOrder(Order order)
        {
            switch (order.Side)
            {
                case OrderSide.Buy:
                    if (BestAsk != null && BestAsk.Price <= order.StopPrice)
                    {
                        switch (order.Type)
                        {
                            case OrderType.StopLimitOrder:
                                return ProcessLimitOrder(order,true);
                            case OrderType.StopMarketOrder:
                                return ProcessMarketOrder(order,true);
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (!StopBids.ContainsKey(order.StopPrice.Value))
                        {
                            StopBids.Add(order.StopPrice.Value, new Queue<Order>());
                        }
                        StopBids[order.StopPrice.Value].Enqueue(order);
                    }
                    break;
                case OrderSide.Sell:
                    if (BestBid != null && BestBid.Price >= order.StopPrice)
                    {
                        switch (order.Type)
                        {
                            case OrderType.StopLimitOrder:
                                return ProcessLimitOrder(order,true);
                            case OrderType.StopMarketOrder:
                                return ProcessMarketOrder(order,true);
                            default:
                                break;
                        }
                    }
                    else
                    {
                        if (!StopBids.ContainsKey(order.StopPrice.Value))
                        {
                            StopBids.Add(order.StopPrice.Value, new Queue<Order>());
                        }
                        StopBids[order.StopPrice.Value].Enqueue(order);
                    }
                    break;
                default:
                    break;
            }

            return new OrderUpdate[] { new OrderUpdate() {
                Placed = true,
                Order = order,
                Message = "Stop order enqueued"
            }};

        }
    }
}
