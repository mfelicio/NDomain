# NDomain

NDomain is a simple, fast, powerful framework to help you build robust and scalable .NET applications using Domain Driven Design, Event Sourcing and CQRS architectures.

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

Here's some basics to get you started, and you can also check the samples.

## Configuring the DomainContext

The `DomainContext` is NDomain's container, where all components are accessible and message processors can be started and stopped.

```c#
var context = DomainContext.Configure()
                           .EventSourcing(c => c.WithAzureTableStorage(azureAccount, "events"))
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

## Creating aggregates

A sample `Aggregate`, enforcing domain rules by checking its state properties and firing state change events

```c#
public class Team : NDomain.Aggregate<TeamState>
{
    public Team(string id, TeamState state)
        : base(id, state)
    {

    }

    public void AddMember(Guid memberId, string memberName)
    {
        if (this.State.Members.ContainsKey(memberId))
        {
            // do nothing, ensures correct idempotency
            return;
        }

        this.On(new TeamMemberAdded { MemberId = memberId, MemberName = memberName });
    }

    // other meaningful operations..
}
```

Aggregate's `State` is changed when events are fired from aggregates, and can be rebuilt by applying all past events, loaded from the `IEventStore`.

```c#
public class TeamState : NDomain.State
{
    //<MemberId, TeamMember>
    public Dictionary<Guid, TeamMember> Members { get; private set; }

    // .. other meaningful properties

    public TeamState()
    {
        this.Members = new Dictionary<Guid, TeamMember>();
    }

    public void OnTeamMemberAdded(TeamMemberAdded ev)
    {
        var member = new TeamMember { Id = ev.MemberId, Name = ev.MemberName, Score = 0 };
        this.Members.Add(member.Id, member);
    }
}
```

Aggregates are loaded and saved by an `IAggregateRepository`, that persists its state change events using the `IEventStore`. As events are persisted, they are also published on the `IEventBus`.


## CQRS handlers and processors

A command handler processes commands sent by the `ICommandBus`, updates aggregates and persists state changes

```c#
public class TeamCommandHandler
{
    readonly IAggregateRepository<Team> repository;
    
    public TeamCommandHandler(IAggregateRepository<Team> repository)
    {
        this.repository = repository;
    }

    public async Task Handle(ICommand<AddTeamMember> command)
    {
        var cmd = command.Payload;

        // perform validations

        await this.repository.Update(cmd.TeamId,
            						 team => team.AddTeamMember(cmd.MemberId, cmd.MemberName));
    }

    // other commands
}
```

An event handler reacts to published events, updates read models used in your queries

```c#
public class TeamEventHandler
{
    
    public async Task On(IEvent<TeamMemberAdded> @event)
    {
        var ev = @event.Payload;

        // do something with it
    }

    // .. other events
}
```


As you can see, NDomain tries to be as less intrusive in your code as much as possible, so you don't need to implement message handler interfaces, as long as you keep the naming conventions.

Message processing is transactional, so if a message handler fails or times out, the message gets back to the queue to be retried. It is important to design your aggregates, command and event handlers to be idempotent.

A processor has an endpoint address (internally a queue) where you can register message handlers, usually for commands and events, but really any POCO can be used as a message. When you register handlers, message subscriptions are created based on the message's Type name, and whenever a message is sent each subscription will get a copy of it, in this case, a processor/handler.

Your commands/event handlers can scale horizontally, as multiple processors using the same endpoint address will process messages from its input queue in a competing consumers fashion.


## License

NDomain is licensed under the [MIT][ndomain-license] license.

[ndomain-license]: http://opensource.org/licenses/mit-license.php
