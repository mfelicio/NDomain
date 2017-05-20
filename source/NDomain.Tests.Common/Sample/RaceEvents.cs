using System;

namespace NDomain.Tests.Common.Sample
{
    public class RaceCreated
    {
        public int MaxDistance { get; set; }
        public int MaxRunners { get; set; }
    }

    public class RunnerJoined
    {
        // these two properties could be grouped into a value object representing a race runner, as they always come together
        public Guid RunnerId { get; set; }
        public string RunnerName { get; set; }
    }

    public class RunnerPositionUpdated
    {
        public Guid RunnerId { get; set; }
        public int Position { get; set; }
    }

    public class RaceStarted
    {
        public DateTime StartTimeUtc { get; set; }
    }

    public class RaceFinished
    {
        public TimeSpan ElapsedTime { get; set; }

        // these two properties could be grouped into a value object representing a race runner, as they always come together
        public Guid WinnerId { get; set; }
        public string WinnerName { get; set; }
    }
}
