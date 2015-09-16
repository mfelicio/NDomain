using NDomain;
using NDomain.CQRS;
using OpenStore.Marketplace.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Domain.Handlers
{
    public class AuctionCommandHandler
    {
        readonly IAggregateRepository<Auction> repository;

        public AuctionCommandHandler(IAggregateRepository<Auction> repository)
        {
            this.repository = repository;
        }

        public async Task Handle(ICommand<CreateAuction> command)
        {
            var cmd = command.Payload;

            await this.repository.CreateOrUpdate(
                                    cmd.AuctionId,
                                    a => a.Create(cmd.SellerId, cmd.Item, cmd.MinPrice, cmd.WindowUtc));
        }

        public async Task Handle(ICommand<PlaceBid> command)
        {
            var cmd = command.Payload;

            await this.repository.Update(cmd.AuctionId,
                                         auction => auction.PlaceBid(cmd.Bid));
        }

        public async Task Handle(ICommand<EndAuction> command)
        {
            var cmd = command.Payload;

            await this.repository.Update(cmd.AuctionId,
                                         auction => auction.EndAuction(DateTime.UtcNow));
        }
    }
}
