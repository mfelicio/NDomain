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
    public class SaleCommandHandler
    {
        readonly IAggregateRepository<Sale> repository;

        public SaleCommandHandler(IAggregateRepository<Sale> repository)
        {
            this.repository = repository;
        }

        public async Task Handle(ICommand<CreateSale> command)
        {
            var cmd = command.Payload;

            await repository.CreateOrUpdate(cmd.SaleId,
                                            s => s.Create(cmd.SellerId, cmd.Item, cmd.Price, cmd.Stock));
        }

        public async Task Handle(ICommand<PlaceOrder> command)
        {
            var cmd = command.Payload;

            await repository.Update(cmd.SaleId, s => s.PlaceOrder(cmd.Order));
        }

        public async Task Handle(ICommand<CompleteOrder> command)
        {
            var cmd = command.Payload;

            await repository.Update(cmd.SaleId, s => s.CompleteOrder(cmd.OrderId));
        }

        public async Task Handle(ICommand<CancelOrder> command)
        {
            var cmd = command.Payload;

            await repository.Update(cmd.SaleId, s => s.CancelOrder(cmd.OrderId));
        }

        public async Task Handle(ICommand<ChangeSaleStock> command)
        {
            var cmd = command.Payload;

            await repository.Update(cmd.SaleId, s => s.ChangeStock(cmd.Value));
        }
    }
}
