using OpenStore.Marketplace.Events;
using OpenStore.Marketplace.Values;
using NDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Domain
{
    public class Auction : Aggregate<AuctionState>
    {
        public Auction(string id, AuctionState state)
            : base(id, state)
        {

        }

        public void Create(string sellerId, Item item, decimal minPrice, Window windowUtc)
        {
            if (State.Created)
            {
                // idempotency
                return;
            }

            this.On(new AuctionCreated
            {
                AuctionId = this.Id,
                SellerId = sellerId,
                Item = item,
                MinPrice = minPrice,
                WindowUtc = windowUtc
            });
        }

        public bool CanPlaceBid(Bid bid)
        {
            return State.Running
                && (State.HigherBid == null || State.HigherBid.Value < bid.Value)
                && State.WindowUtc.Comprises(bid.DateUtc);
        }

        /// <summary>
        /// Places a new bid on the auction
        /// </summary>
        /// <param name="bid">bid</param>
        public void PlaceBid(Bid bid)
        {
            if (State.Bids.ContainsKey(bid.Id))
            {
                // idempotency
                return;
            }

            if (!CanPlaceBid(bid))
            {
                // should return error code
                return;
            }

            this.On(new BidPlaced { AuctionId = this.Id, Bid = bid });
        }

        /// <summary>
        /// Terminates an auction, which may have or not a winner
        /// </summary>
        public void EndAuction(DateTime dateUtc)
        {
            if (!State.Running)
            {
                return;
            }

            var hasWinner = State.HigherBid != null && State.HigherBid.Value >= State.MinPrice;

            this.On(new AuctionEnded
            {
                AuctionId = this.Id,
                DateUtc = dateUtc,
                HigherBid = State.HigherBid,
                HasWinner = hasWinner
            });
        }
    }
}
