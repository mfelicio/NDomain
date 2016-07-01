using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NDomain.Logging;
using NDomain.Bus.Transport;

namespace NDomain.Bus
{
    /// <summary>
    /// Handles all the logic to receive messages from an IInboundTransport in a message loop.
    /// Handles success and failure scenarios.
    /// Actual message processing is delegated to the IMessageDispatcher.
    /// </summary>
    public class MessageWorker : IDisposable
    {
        readonly IMessageDispatcher messageDispatcher;
        readonly IInboundTransport receiver;
        readonly ILogger logger;

        readonly CancellationTokenSource cancellation;
        readonly int concurrencyLevel;

        readonly ManualResetEventSlim workWaitHandle;
        long workCount;

        readonly Task workerTask;
        readonly ManualResetEventSlim runningWaitHandle;

        public MessageWorker(IInboundTransport receiver,
                             IMessageDispatcher messageDispatcher,
                             ILoggerFactory loggerFactory,
                             int concurrencyLevel = 200)
        {
            this.receiver = receiver;
            this.messageDispatcher = messageDispatcher;
            this.logger = loggerFactory.GetLogger(typeof(MessageWorker));
            this.cancellation = new CancellationTokenSource();
            this.concurrencyLevel = concurrencyLevel;

            this.workWaitHandle = new ManualResetEventSlim(true);
            this.workCount = 0;

            this.runningWaitHandle = new ManualResetEventSlim(false);
            this.workerTask = Task.Factory.StartNew(async () => await this.Work(), TaskCreationOptions.LongRunning);
        }

        public void Start()
        {
            this.runningWaitHandle.Set();
        }

        public void Stop()
        {
            this.runningWaitHandle.Reset();
        }

        public bool IsRunning
        {
            get { return this.runningWaitHandle.IsSet; }
        }

        private async Task Work()
        {
            // message loop
            while (!this.cancellation.IsCancellationRequested)
            {
                if (!this.IsRunning)
                {
                    // wait 1 second before checking again
                    WaitForRunSignal();
                    continue;
                }

                if (!HasSlots())
                {
                    WaitForSlots();
                }
                else
                {
                    var transaction = await this.GetMessage();
                    if (transaction != null)
                    {
                        AcquireSlot();

                        Task.Run(() => this.ProcessTransaction(transaction))
                            .ContinueWith(t => ReleaseSlot());
                    }
                }
            }
        }

        private void WaitForSlots()
        {
            this.workWaitHandle.Wait(1000);
        }

        private bool HasSlots()
        {
            return this.workWaitHandle.IsSet && Interlocked.Read(ref workCount) < this.concurrencyLevel;
        }

        private void WaitForRunSignal()
        {
            this.runningWaitHandle.Wait(1000);
        }

        private async Task<IMessageTransaction> GetMessage()
        {
            IMessageTransaction transaction = null;

            try
            {
                transaction = await this.receiver.Receive(TimeSpan.FromSeconds(60));
            }
            catch (Exception ex)
            {
                // log error
                // TODO: add some receiver info like Queue?
                this.logger.Error(ex, "Failed to receive message");
            }

            return transaction;
        }

        private void AcquireSlot()
        {
            if (Interlocked.Increment(ref this.workCount) == this.concurrencyLevel)
            {
                if (this.workWaitHandle.IsSet)
                {
                    this.workWaitHandle.Reset();
                }
            }
        }

        private void ReleaseSlot()
        {
            Interlocked.Decrement(ref this.workCount);

            if (!this.workWaitHandle.IsSet)
            {
                this.workWaitHandle.Set();
            }
        }

        private async Task ProcessTransaction(IMessageTransaction transaction)
        {
            Func<Task> completion;

            try
            {
                using (new DomainTransactionScope(transaction.Message.Id, transaction.DeliveryCount))
                {
                    // process message within a logical transaction that can be used for processing idempotency
                    await this.messageDispatcher.ProcessMessage(transaction.Message);
                }

                completion = () => transaction.Commit();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex,
                                     "Failed to process message {0} with id {1}",
                                     transaction.Message.Name,
                                     transaction.Message.Id);

                completion = () => transaction.Fail();
            }

            // try to complete the transaction, regardless of failures
            try
            {
                await completion();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex,
                                     "Failed to complete message {0} with id {1}",
                                     transaction.Message.Name,
                                     transaction.Message.Id);
            }
        }

        public void Dispose()
        {
            if (!this.cancellation.IsCancellationRequested)
            {
                if (this.IsRunning)
                {
                    this.Stop();
                }

                this.cancellation.Cancel();
                this.workerTask.Wait();
            }
        }
    }
}
