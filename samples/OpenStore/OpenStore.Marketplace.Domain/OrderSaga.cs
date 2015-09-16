using OpenStore.Marketplace.Commands;
using OpenStore.Marketplace.Events;
using NDomain.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Marketplace.Domain
{
    // This is a work in progress, not yet supported by NDomain
    public class OrderSaga
    {
        readonly ICommandBus commandBus;

        public OrderSaga(ICommandBus commandBus)
        {
            this.commandBus = commandBus;
        }

        public async Task On(IEvent<OrderPlaced> ev)
        {
            // eventually payment integration events could be here

            // send complete order
        }

        public async Task On(IEvent<OrderCompleted> ev)
        {
            // everything is done now

            // complete saga
        }

        public async Task On(IEvent<OrderCancelled> ev)
        {
            // eventually refund

            // complete saga
        }
    }
}
