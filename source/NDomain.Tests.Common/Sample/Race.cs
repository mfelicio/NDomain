using System;

namespace NDomain.Tests.Common.Sample
{
    /// <summary>
    /// Sample aggregate for testing purposes..
    /// A race is created with a specific maximum distance and with a specific number of runners
    /// When enough runners join the race, the race be started
    /// Runners can only start running once the race has started
    /// When a runner reaches the max distance, he is the winner and the race is finished 
    /// When a race finishes, an event should be published with the winner and the elapsed time since the race started
    /// </summary>
    public class Race : NDomain.Model.EventSourcedAggregate<RaceState>
    {
        public Race(string id, RaceState state)
            : base(id, state)
        {

        }

        public void Create(int maxDistance, int maxRunners)
        {
            if (this.State.Created)
            {
                // do nothing, enforces idempotency
                return;
            }

            this.On(new RaceCreated { MaxDistance = maxDistance, MaxRunners = maxRunners });
        }

        public void AddRunner(Guid runnerId, string runnerName)
        {
            if (this.State.Runners.ContainsKey(runnerId))
            {
                // do nothing, ensures correct idempotency
                // must be checked before maxRunners condition is enforced
                return;
            }

            if (this.State.Runners.Count >= this.State.MaxRunners)
            {
                // domain error
                throw new InvalidOperationException("Maximum number of runners already reached");
            }

            this.On(new RunnerJoined { RunnerId = runnerId, RunnerName = runnerName });
        }

        public void Start(DateTime startTimeUtc)
        {
            if (this.State.Runners.Count < this.State.MaxRunners)
            {
                // domain error
                throw new InvalidOperationException("Not enough runners joined the race for it to be started");
            }

            if (this.State.Started)
            {
                // do nothing, ensures correct idempotency
                return;
            }

            this.On(new RaceStarted { StartTimeUtc = startTimeUtc });
        }

        public void UpdatePosition(Guid runnerId, int position)
        {
            RaceRunner runner;
            if (!this.State.Runners.TryGetValue(runnerId, out runner))
            {
                // domain error
                throw new InvalidOperationException("Runner is not in the race");
            }

            if (!this.State.Started)
            {
                // domain error
                throw new InvalidOperationException("Race hasn't started yet");
            }

            // a runner can only run forward
            if (runner.Position >= position)
            {
                // do nothing, enforces correct idempotency by checking 
                //if the runner is already at this position or even further
                return;
            }

            // firing this event effectively updates the state
            this.On(new RunnerPositionUpdated { RunnerId = runnerId, Position = position });

            // check if we have a winner
            if (runner.Position >= this.State.MaxDistance)
            {
                var nowUtc = DateTime.UtcNow;
                var elapsed = nowUtc - this.State.StartTimeUtc;

                this.On(
                    new RaceFinished
                    {
                        WinnerId = runner.Id,
                        WinnerName = runner.Name,
                        ElapsedTime = elapsed
                    });
            }
        }
    }
}
