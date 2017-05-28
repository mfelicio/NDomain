# NDomain

[![Build status](https://ci.appveyor.com/api/projects/status/rovai946s6awhoeq?svg=true)](https://ci.appveyor.com/project/mfelicio/ndomain)

NDomain is an extensible, fast, powerful framework to help you build robust and scalable .NET applications using Domain Driven Design, Event Sourcing and CQRS architectures.

## Features:

* Robust EventStore implementation, where events can be stored and published using different technologies
* Base aggregate class, whose state that can be rebuilt from stored events or from a snapshot
* Repository to load/save aggregates, so you get true persistence ignorance in your domain layer
* Brokerless message bus with transports for multiple technologies including Redis and Azure Queues
* CommandBus and EventBus built on top of the message bus, as well as Command and Event handlers
* Integration with your logging and IoC container
* Fully async to its core, leveraging non blocking IO operations and keeping resource usage to a minimum
* Naming convention based, meaning you don't need to implement interfaces for each command/event handler, it just works and it's fast!
* No reflection to invoke command/event handlers nor rebuilding aggregates, compiled lambda expression trees are created on startup
* In-proc implementations for all components, so you can decide to move to a distributed architecture later without having to refactor your whole solution.
* A straightforward Fluent configuration API, to let you choose the implementation of each component
* A suite of base unit test classes, so that all different implementations for a given component are tested in the same way

## Great, how does it work?

Here's some basics to get you started, and you can also check the samples.

### Configuring the DomainContext

The `DomainContext` is NDomain's container, where all components are accessible and message processors can be started and stopped.

```c#
var context = DomainContext.Configure()
                           .Model(c => c.WithAzureTableStorage(azureAccount, "events"))
                           .Logging(c => c.WithNLog())
                           .IoC(c => c.WithAutofac(container))
                           .Bus(c => c.WithAzureQueues(azureAccount)
                                      .WithRedisSubscriptionStore(redisConnection)
                                      .WithRedisSubscriptionBroker(redisConnection)
                                      .WithProcessor(p => p.Endpoint("background-worker")
                                                           .RegisterHandler<CommandHandlerThatUpdatesSomeAggregate>()
                                                           .RegisterHandler<EventHandlerThatUpdatesAReadModel>()
                                                           .RegisterHandler<EventHandlerThatUpdatesAnotherReadModel>()))
                           .Start();
```

`DomainContext` exposes an `ICommandBus`, `IEventBus`, `IEventStore` and `IAggregateRepository` that you can use by either passing the `DomainContext` around or if you use an IoC container you can just configure it and depend on them.

### Creating aggregates

A sample `Aggregate`, enforcing domain rules by checking its state properties and firing state change events

```c#
public class Sale : Aggregate<SaleState>
{
    public Sale(string id, SaleState state) : base(id, state)  { }

    public bool CanPlaceOrder(Order order)
    {
        return State.AvailableStock >= order.Quantity;
    }

    public void PlaceOrder(Order order)
    {
        if (State.PendingOrders.ContainsKey(order.Id))
        {
            // idempotency
            return;
        }

        if (!CanPlaceOrder(order))
        {
            // return error code or throw exception
            throw new InvalidOperationException("not enough quantity");
        }

        this.On(new OrderPlaced { SaleId = this.Id, Order = order});
    }

    public void CancelOrder(string orderId)
    {
        if (!State.PendingOrders.ContainsKey(orderId))
        {
            // idempotency
            return;
        }

        this.On(new OrderCancelled { SaleId = this.Id, OrderId = orderId });
    }

    // check OpenStore samples for complete example
}
```

Aggregate's `State` is changed when events are fired from aggregates, and can be rebuilt by applying all past events, loaded from the `IEventStore`.

```c#
public class SaleState : State
{
    public string SellerId { get; set; }
    public Item Item { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int AvailableStock { get; set; }

    public Dictionary<string, Order> PendingOrders { get; set; }

    public SaleState()
    {
        this.PendingOrders = new Dictionary<string, Order>();
    }

    private void On(SaleCreated ev)
    {
        this.SellerId = ev.SellerId;
        this.Item = ev.Item;
        this.Price = ev.Price;
        this.Stock = this.AvailableStock = ev.Stock;
    }

    private void On(OrderPlaced ev)
    {
        AvailableStock -= ev.Order.Quantity;
        PendingOrders[ev.Order.Id] = ev.Order;
    }

    private void On(OrderCancelled ev)
    {
        AvailableStock += PendingOrders[ev.OrderId].Quantity;
        PendingOrders.Remove(ev.OrderId);
    }

    private void On(OrderCompleted ev)
    {
        var order = PendingOrders[ev.OrderId];
        
        Stock -= order.Quantity;
        PendingOrders.Remove(ev.OrderId);
    }
    
    // check OpenStore samples for complete example
}
```

Aggregates are loaded and saved by an `IAggregateRepository`, that persists its state change events using the `IEventStore`. As events are persisted, they are also published on the `IEventBus`.


### CQRS handlers and processors

A command handler processes commands sent by the `ICommandBus`, updates aggregates and persists state changes

```c#
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

    // other commands
}
```

An event handler reacts to published events, updates read models used in your queries

```c#
public class SaleEventHandler
{
    
    public async Task On(IEvent<OrderCompleted> @event)
    {
        var ev = @event.Payload;

        // do something with it
    }

    // .. other events
}
```


As you can see, NDomain tries to be as less intrusive in your code as much as possible, so you don't need to implement message handler interfaces, as long as you keep the naming conventions.

Message processing is transactional, so if a message handler fails or times out, the message gets back to the queue to be retried. It is important to design your aggregates, command and event handlers to be idempotent to avoid side effects.

A processor has an endpoint address (internally a queue) where you can register message handlers, usually for commands and events, but really any POCO can be used as a message. When you register handlers, message subscriptions are created based on the message's Type name, and whenever a message is sent each subscription will get a copy of it, in this case, a processor/handler.

Your commands/event handlers can scale horizontally, as multiple processors using the same endpoint address will process messages from its input queue in a competing consumers fashion.

## Contributing

If you would like to have support for other technologies, please take a look at the existing implementations and feel free to implement your own and submit a pull request. NDomain's source code is very clean and simple, let's keep it that way!

For bugs, improvements or new feature suggestions just open a new Issue so we can track it.

## License

NDomain is licensed under the [MIT][ndomain-license] license.

[ndomain-license]: http://opensource.org/licenses/mit-license.php
