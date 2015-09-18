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
    public class AuctionState : State
    {
        public bool Created { get; set; }
        
        public string SellerId { get; set; }
        public Item Item { get; set; }
        public decimal MinPrice { get; set; }
        public Window WindowUtc { get; set; }

        public bool Running { get; set; }

        public Dictionary<string, Bid> Bids { get; set; }
        public Bid HigherBid { get; set; }

        public AuctionState()
        {
            this.Bids = new Dictionary<string, Bid>();
        }

        private void OnAuctionCreated(AuctionCreated ev)
        {
            this.SellerId = ev.SellerId;
            this.Item = ev.Item;
            this.MinPrice = ev.MinPrice;
            this.WindowUtc = ev.WindowUtc;
            this.Running = true;

            this.Created = true;
        }

        private void OnBidPlaced(BidPlaced ev)
        {
            this.Bids[ev.Bid.Id] = ev.Bid;
            this.HigherBid = ev.Bid;
        }

        private void OnAuctionEnded(AuctionEnded ev)
        {
            this.Running = false;
        }
    }
}
