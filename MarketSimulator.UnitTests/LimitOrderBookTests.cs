using System;
using MarketSimulator.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MarketSimulator.UnitTests
{
    [TestClass]
    public class LimitOrderBookTests
    {
        [TestMethod]
        public void CanInstantiateLOB()
        {
            var lob = new LimitOrderBook.StandardLimitOrderBook();

            Assert.IsNotNull(lob);

            Assert.IsInstanceOfType(lob, typeof(ILimitOrderBook));
        }

        [TestMethod]
        public void CanSubmitBuyLimitOrder()
        {
            ILimitOrderBook lob = new LimitOrderBook.StandardLimitOrderBook();

            Assert.IsNotNull(lob);
            Assert.AreEqual(0, lob.Bids.Count);

            Assert.IsInstanceOfType(lob, typeof(ILimitOrderBook));

            var testOrder = new Order() { Price = 100, Quantity = 10, Side = OrderSide.Buy, Type = OrderType.LimitOrder, UserID = "Test" };
            var testOrderGuid = testOrder.ID.ToString();

            var result = lob.ProcessLimitOrder(testOrder,true);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);
            Assert.AreEqual("Test", result.First().Order.UserID);
            Assert.AreEqual(testOrderGuid, result.First().Order.ID.ToString());
            Assert.AreEqual(OrderSide.Buy, result.First().Order.Side);
            Assert.AreEqual(1, lob.Bids.Count);

            Assert.AreEqual(100, lob.BestBid.Price);
            Assert.AreEqual(10, lob.BestBid.Quantity);
        }

        [TestMethod]
        public void CanSubmitSellLimitOrder()
        {
            ILimitOrderBook lob = new LimitOrderBook.StandardLimitOrderBook();

            Assert.IsNotNull(lob);
            Assert.AreEqual(0, lob.Bids.Count);

            Assert.IsInstanceOfType(lob, typeof(ILimitOrderBook));

            var testOrder = new Order() { Price = 100, Quantity = 10, Side = OrderSide.Sell, Type = OrderType.LimitOrder, UserID = "Test" };
            var testOrderGuid = testOrder.ID.ToString();

            var result = lob.ProcessLimitOrder(testOrder,true);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);
            Assert.AreEqual("Test", result.First().Order.UserID);
            Assert.AreEqual(testOrderGuid, result.First().Order.ID.ToString());
            Assert.AreEqual(OrderSide.Sell, result.First().Order.Side);
            Assert.AreEqual(1, lob.Asks.Count);


            Assert.AreEqual(100, lob.BestAsk.Price);
            Assert.AreEqual(10, lob.BestAsk.Quantity);
        }

        //[TestMethod]
        //public void CanSubmitBuyMarketOrderSimple()
        //{
        //    //setup
        //    ILimitOrderBook lob = new LimitOrderBook.StandardLimitOrderBook();

        //    Assert.IsNotNull(lob);
        //    Assert.AreEqual(0, lob.Bids.Count);

        //    Assert.IsInstanceOfType(lob, typeof(ILimitOrderBook));

        //    var testOrder = new Order() { Price = 100, Quantity = 10, Side = OrderSide.Sell, Type = OrderType.LimitOrder, UserID = "Test" };
        //    var testOrderGuid = testOrder.ID.ToString();

        //    var result1 = lob.ProcessLimitOrder(testOrder);

        //    Assert.IsNotNull(result1);
        //    Assert.AreEqual(1, result1.Count());
        //    Assert.AreEqual(true, result1.First().Placed);
        //    Assert.AreEqual("Test", result1.First().Order.UserID);
        //    Assert.AreEqual(testOrderGuid, result1.First().Order.ID.ToString());
        //    Assert.AreEqual(OrderSide.Sell, result1.First().Order.Side);
        //    Assert.AreEqual(1, lob.Asks.Count);

        //    //act
        //    var marketOrder = new Order()
        //    {
        //        Quantity = 4,
        //        Side = OrderSide.Buy,
        //        Type = OrderType.MarketOrder,
        //        UserID = "Test2"
        //    };

        //    var result = lob.ProcessMarketOrder(marketOrder);

        //    Assert.AreEqual(2, result.Count());

        //    var limitOrderResult = result.First(o => o.Order.ID == testOrder.ID);
        //    var marketOrderResults = result.First(o => o.Order.ID == marketOrder.ID);

        //    Assert.AreEqual(true, marketOrderResults.Placed);
        //    Assert.AreEqual(1, marketOrderResults.Matches.Count());
        //    Assert.AreEqual(4, marketOrderResults.Matches[0].Quantity);
        //    Assert.AreEqual(100, marketOrderResults.Matches[0].Price);

        //    Assert.AreEqual(1, limitOrderResult.Matches.Count());
        //    Assert.AreEqual(4, limitOrderResult.Matches[0].Quantity);
        //    Assert.AreEqual(100, limitOrderResult.Matches[0].Price);

        //    Assert.AreEqual(6, lob.Asks.First().Value.First().Quantity);
        //}

        //[TestMethod]
        //public void CanSubmitSellMarketOrderSimple()
        //{
        //    //setup
        //    ILimitOrderBook lob = new LimitOrderBook.StandardLimitOrderBook();

        //    Assert.IsNotNull(lob);
        //    Assert.AreEqual(0, lob.Bids.Count);

        //    Assert.IsInstanceOfType(lob, typeof(ILimitOrderBook));

        //    var testOrder = new Order() { Price = 100, Quantity = 10, Side = OrderSide.Buy, Type = OrderType.LimitOrder, UserID = "Test" };
        //    var testOrderGuid = testOrder.ID.ToString();

        //    var result1 = lob.ProcessLimitOrder(testOrder);

        //    Assert.IsNotNull(result1);
        //    Assert.AreEqual(1, result1.Count());
        //    Assert.AreEqual(true, result1.First().Placed);
        //    Assert.AreEqual("Test", result1.First().Order.UserID);
        //    Assert.AreEqual(testOrderGuid, result1.First().Order.ID.ToString());
        //    Assert.AreEqual(OrderSide.Buy, result1.First().Order.Side);
        //    Assert.AreEqual(1, lob.Bids.Count);

        //    //act
        //    var marketOrder = new Order()
        //    {
        //        Quantity = 4,
        //        Side = OrderSide.Sell,
        //        Type = OrderType.MarketOrder,
        //        UserID = "Test2"
        //    };

        //    var result = lob.ProcessMarketOrder(marketOrder);

        //    Assert.AreEqual(2, result.Count());

        //    var limitOrderResult = result.First(o => o.Order.ID == testOrder.ID);
        //    var marketOrderResults = result.First(o => o.Order.ID == marketOrder.ID);

        //    Assert.AreEqual(true, marketOrderResults.Placed);
        //    Assert.AreEqual(1, marketOrderResults.Matches.Count());
        //    Assert.AreEqual(4, marketOrderResults.Matches[0].Quantity);
        //    Assert.AreEqual(100, marketOrderResults.Matches[0].Price);

        //    Assert.AreEqual(1, limitOrderResult.Matches.Count());
        //    Assert.AreEqual(4, limitOrderResult.Matches[0].Quantity);
        //    Assert.AreEqual(100, limitOrderResult.Matches[0].Price);

        //    Assert.AreEqual(6, lob.Bids.First().Value.First().Quantity);
        //}

        [TestMethod]
        public void CanSubmitBuyMarketOrderSimple()
        {
            //ARRANGE
            ILimitOrderBook lob = new LimitOrderBook.StandardLimitOrderBook();

            Assert.IsNotNull(lob);
            Assert.AreEqual(0, lob.Bids.Count);

            Assert.IsInstanceOfType(lob, typeof(ILimitOrderBook));

            var setupOrder = new Order() { Price = 101, Quantity = 20, Side = OrderSide.Sell, Type = OrderType.LimitOrder, UserID = "Test" };           
            var result = lob.ProcessLimitOrder(setupOrder,true);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);

            setupOrder = new Order() { Price = 101, Quantity = 10, Side = OrderSide.Sell, Type = OrderType.LimitOrder, UserID = "Test" };
            result = lob.ProcessLimitOrder(setupOrder, true);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);

            setupOrder = new Order() { Price = 103, Quantity = 30, Side = OrderSide.Sell, Type = OrderType.LimitOrder, UserID = "Test" };
            result = lob.ProcessLimitOrder(setupOrder, true);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);

            setupOrder = new Order() { Price = 104, Quantity = 15, Side = OrderSide.Sell, Type = OrderType.LimitOrder, UserID = "Test" };
            result = lob.ProcessLimitOrder(setupOrder, true);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);

            setupOrder = new Order() { Price = 104, Quantity = 5, Side = OrderSide.Sell, Type = OrderType.LimitOrder, UserID = "Test" };
            result = lob.ProcessLimitOrder(setupOrder, true);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);

            setupOrder = new Order() { Price = 105, Quantity = 50, Side = OrderSide.Sell, Type = OrderType.LimitOrder, UserID = "Test" };
            result = lob.ProcessLimitOrder(setupOrder, true);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);

            setupOrder = new Order() { Price = 94, Quantity = 30, Side = OrderSide.Buy, Type = OrderType.LimitOrder, UserID = "Test" };
            result = lob.ProcessLimitOrder(setupOrder, true);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);

            setupOrder = new Order() { Price = 96, Quantity = 50, Side = OrderSide.Buy, Type = OrderType.LimitOrder, UserID = "Test" };
            result = lob.ProcessLimitOrder(setupOrder, true);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);

            setupOrder = new Order() { Price = 97, Quantity = 40, Side = OrderSide.Buy, Type = OrderType.LimitOrder, UserID = "Test" };
            result = lob.ProcessLimitOrder(setupOrder, true);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);

            setupOrder = new Order() { Price = 97, Quantity = 10, Side = OrderSide.Buy, Type = OrderType.LimitOrder, UserID = "Test" };
            result = lob.ProcessLimitOrder(setupOrder, true);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);

            setupOrder = new Order() { Price = 97, Quantity = 10, Side = OrderSide.Buy, Type = OrderType.LimitOrder, UserID = "Test" };
            result = lob.ProcessLimitOrder(setupOrder, true);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(true, result.First().Placed);


            //ACT
            var marketOrder = new Order()
            {
                Quantity = 15,
                Side = OrderSide.Buy,
                Type = OrderType.MarketOrder,
                UserID = "Test2"
            };

            var testResult = lob.ProcessMarketOrder(marketOrder, true);

            Assert.AreEqual(1, result.Count());           

            //ASSERT

            Assert.AreEqual(2, lob.Asks[101].Count);
            Assert.AreEqual(5,lob.Asks[101].ElementAt(0).Quantity);
            Assert.AreEqual(10, lob.Asks[101].ElementAt(1).Quantity);

            Assert.AreEqual(1, lob.Asks[103].Count);
            Assert.AreEqual(30, lob.Asks[103].ElementAt(0).Quantity);

            Assert.AreEqual(2, lob.Asks[104].Count);
            Assert.AreEqual(15, lob.Asks[104].ElementAt(0).Quantity);
            Assert.AreEqual(5, lob.Asks[104].ElementAt(1).Quantity);

            Assert.AreEqual(1, lob.Asks[105].Count);
            Assert.AreEqual(50, lob.Asks[105].ElementAt(0).Quantity);

            Assert.AreEqual(3, lob.Bids[97].Count);
            Assert.AreEqual(40, lob.Bids[97].ElementAt(0).Quantity);
            Assert.AreEqual(10, lob.Bids[97].ElementAt(1).Quantity);
            Assert.AreEqual(10, lob.Bids[97].ElementAt(2).Quantity);

            Assert.AreEqual(1, lob.Bids[96].Count);
            Assert.AreEqual(50, lob.Bids[96].ElementAt(0).Quantity);

            Assert.AreEqual(1, lob.Bids[94].Count);
            Assert.AreEqual(30, lob.Bids[94].ElementAt(0).Quantity);
            
        }
    }
}
