using NDomain.Bus.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace NDomain.Bus
{
    // TODO: Remove. This should go to extensibility/diagnostics
    class Counter
    {
        public long NMessages;
        public long TotalMS;

        public TimeSpan GetAvg()
        {
            var time = Interlocked.Read(ref TotalMS) / Interlocked.Read(ref NMessages);
            return TimeSpan.FromMilliseconds(time);
        }

        public override string ToString()
        {
            return GetAvg().ToString();
        }
    }

    internal class DiagnosticsDispatcher : IMessageDispatcher
    {
        readonly IMessageDispatcher innerDispatcher;
        readonly ConcurrentDictionary<string, Counter> metrics;

        public DiagnosticsDispatcher(IMessageDispatcher innerDispatcher)
        {
            this.innerDispatcher = innerDispatcher;
            this.metrics = new ConcurrentDictionary<string, Counter>();
        }

        public async Task ProcessMessage(TransportMessage message)
        {

            var sw = Stopwatch.StartNew();
            await this.innerDispatcher.ProcessMessage(message);
            sw.Stop();

            var key = string.Format("{0}:{1}", message.Name, message.Headers[MessageHeaders.Component]);
            var counter = this.metrics.GetOrAdd(key, k => new Counter());
            Interlocked.Increment(ref counter.NMessages);
            Interlocked.Add(ref counter.TotalMS, sw.ElapsedMilliseconds);

            Trace.WriteLine(string.Format("Processed {0} in {1}ms with avg {2}ms", key, sw.ElapsedMilliseconds, counter.GetAvg().TotalMilliseconds));
        }
    }
}
