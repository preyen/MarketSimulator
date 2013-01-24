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

        public Order BestBid
        {
            get { return new Order { Price = Bids.Last().Key, Quantity = Bids.Last().Value.Sum(b => b.Quantity) }; }
        }

        public Order BestAsk
        {
            get { return new Order { Price = Asks.First().Key, Quantity = Asks.First().Value.Sum(a => a.Quantity)}; }
        }

        public IEnumerable<OrderUpdate> ProcessLimitOrder(Order order)
        {
            switch (order.Side)
            {
                case OrderSide.Buy:
                    return ProcessBuyLimitOrder(order);
                case OrderSide.Sell:
                    return ProcessSellLimitOrder(order);
                default:
                    return new[] {new OrderUpdate()
                    {
                        Placed = false,
                        Order = order,
                        Message = "Order side not known"
                    }};
            }
        }

        private IEnumerable<OrderUpdate> ProcessSellLimitOrder(Order order)
        {
            /*if (Bids.Any() && price < Bids.Last().Key)
            {
                //cross order
                return false;
            }*/
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
            /*if (Asks.Any() && price > Asks.First().Key)
            {
                //cross order
                return false;
            }*/
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
            throw new NotImplementedException();
        }

        public IEnumerable<OrderUpdate> ProcessMarketOrder(Order order)
        {
            switch (order.Side)
            {
                case OrderSide.Buy:
                    return ProcessBuyMarketOrder(order);
                case OrderSide.Sell:
                    return ProcessSellMarketOrder(order);
                default:
                    return new[] {new OrderUpdate()
                    {
                        Placed = false,
                        Order = order,
                        Message = "Order side not known"
                    }};
            }
        }

        private IEnumerable<OrderUpdate> ProcessSellMarketOrder(Order order)
        {
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
            throw new NotImplementedException();

            var orderToCancel = Bids.SelectMany(s => s.Value).Union(Asks.SelectMany(s => s.Value)).FirstOrDefault(o => o.ID == order.ID);

            if (orderToCancel != null)
            {
                
            }
        }
    }
}
