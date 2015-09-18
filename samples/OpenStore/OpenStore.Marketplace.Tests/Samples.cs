using NDomain;
using OpenStore.Marketplace.Domain;
using OpenStore.Marketplace.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Tests
{
    public class Samples
    {
        public static readonly Window YesterdayAndTomorrowWindow =
            new Window
            {
                Start = DateTime.Now.AddDays(-1),
                End = DateTime.Now.AddDays(1)
            };

        public static Auction CreateTestAuction(decimal? minPrice = null,
                                         Window window = null)
        {
            var auction = new Auction("test-auction-id", new AuctionState());

            var price = minPrice ?? 10;
            window = window ?? new Window { Start = DateTime.UtcNow.AddDays(-1), End = DateTime.UtcNow.AddDays(1) };

            auction.Create(
                    "test-seller-id",
                    new Item { Id = "test-item-id", Name = "item name" },
                    price,
                    window);

            // create a new aggregate from the state, to ensure the aggregate doesn't come with changes
            return AggregateFactory.For<Auction>().CreateFromState(auction.Id, auction.State);
        }

        public static Bid CreateTestBid(decimal value, DateTime? dateUtc = null)
        {
            return new Bid
            {
                Id = Guid.NewGuid().ToString(),
                MemberId = Guid.NewGuid().ToString(),
                Value = value,
                DateUtc = dateUtc ?? DateTime.UtcNow
            };
        }

        public static Order CreateTestOrder(int quantity, DateTime? dateUtc = null)
        {
            return new Order
            {
                Id = Guid.NewGuid().ToString(),
                MemberId = Guid.NewGuid().ToString(),
                Quantity = quantity,
                DateUtc = dateUtc ?? DateTime.UtcNow
            }; 
        }

        public static Sale CreateSale(int? saleStock = null)
        {
            var stock = saleStock ?? 10;

            var sale = new Sale("test-sale-id", new SaleState());

            sale.Create("test-seller-id",
                        new Values.Item { Id = "item-id", Name = "item-name" }, 
                        price: 10, 
                        stock: stock);

            // create a new aggregate from the state, to ensure the aggregate doesn't come with changes
            return AggregateFactory.For<Sale>().CreateFromState(sale.Id, sale.State);
        }
    }
}
