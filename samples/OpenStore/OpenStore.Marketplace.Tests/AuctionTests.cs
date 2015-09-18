using NDomain;
using NUnit.Framework;
using OpenStore.Marketplace.Domain;
using OpenStore.Marketplace.Events;
using OpenStore.Marketplace.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Tests
{
    public class AuctionTests
    {
        /// <summary>
        /// Testing the happy path here
        /// </summary>
        [TestCase(10, 10, true)]
        [TestCase(10, 15, true)]
        [TestCase(10, 5, false)]
        public void EndAuction_Should_HaveWinner_WhenHasBidsAboveOrEqualMinPrice(decimal minPrice, decimal bidValue, bool winner)
        {
            // arrange
            var auction = Samples.CreateTestAuction(minPrice: minPrice);
            auction.PlaceBid(Samples.CreateTestBid(value: bidValue));

            // act
            auction.EndAuction(DateTime.UtcNow);

            // assert
            var @event = auction.Changes.Last().Payload as AuctionEnded;
            Assert.AreEqual(winner, @event.HasWinner);
        }

        [Test]
        public void EndAuction_ShouldNot_HaveWinnerOrHigherBid_WhenNoBidsArePlaced()
        {
            // arrange
            var auction = Samples.CreateTestAuction();

            //act
            auction.EndAuction(DateTime.Now);

            // assert
            Assert.That(auction.Changes.Count(), Is.EqualTo(1));
            Assert.That(auction.Changes.First().Payload, Is.InstanceOf<AuctionEnded>());

            var @event = auction.Changes.First().Payload as AuctionEnded;
            Assert.That(@event.HasWinner, Is.False);
            Assert.That(@event.HigherBid, Is.Null);
        }

        [TestCase(10, 15, true)]
        [TestCase(10, 10, false)]
        [TestCase(10, 5, false)]
        public void PlaceBid_Should_PlaceBid_WhenValueAboveHigherBid(decimal higherBidValue, decimal newBidValue, bool should)
        {
            // arrange
            var auction = Samples.CreateTestAuction();
            auction.PlaceBid(Samples.CreateTestBid(value: higherBidValue)); // initial bid

            // act
            auction.PlaceBid(Samples.CreateTestBid(value: newBidValue));

            // assert
            Assert.That(auction.Changes.Count(), Is.EqualTo(should ? 2 : 1)); // two or one bid
        }

        [Test]
        public void CreateAuction_Is_Idempotent()
        {
            // arrange
            var auction = new Auction("a1", new AuctionState());

            // act
            auction.Create("s1", new Item(), 10, Samples.YesterdayAndTomorrowWindow);
            auction.Create("s1", new Item(), 10, Samples.YesterdayAndTomorrowWindow);

            // assert
            Assert.That(auction.Changes.Count(), Is.EqualTo(1));
            Assert.That(auction.Changes.First().Payload, Is.InstanceOf<AuctionCreated>());
        }

        [Test]
        public void PlaceBid_Is_Idempotent()
        {
            // arrange
            var auction = Samples.CreateTestAuction();

            // act
            auction.PlaceBid(new Bid { Id = "b1", MemberId = "test-member", Value = 10, DateUtc = DateTime.Now });
            auction.PlaceBid(new Bid { Id = "b1", MemberId = "test-member", Value = 10, DateUtc = DateTime.Now });

            // assert
            Assert.That(auction.Changes.Count(), Is.EqualTo(1));
            Assert.That(auction.Changes.First().Payload, Is.InstanceOf<BidPlaced>());
        }

        [Test]
        public void EndAuction_Is_Idempotent()
        {
            // arrange
            var auction = Samples.CreateTestAuction();

            // act
            auction.EndAuction(DateTime.Now);
            auction.EndAuction(DateTime.Now);

            // assert
            Assert.That(auction.Changes.Count(), Is.EqualTo(1));
            Assert.That(auction.Changes.First().Payload, Is.InstanceOf<AuctionEnded>());
        }
    }
}
