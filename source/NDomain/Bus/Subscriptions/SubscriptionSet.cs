using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Subscriptions
{
    /// <summary>
    /// Helper structure to manage subscription changes
    /// </summary>
    class SubscriptionSet
    {
        readonly HashSet<Subscription> subscriptions;

        // readonly copy of the current subscriptions to allow other threads to read them while the HashSet subscriptions is modified by others
        private Subscription[] latestCopy;

        public SubscriptionSet()
        {
            this.subscriptions = new HashSet<Subscription>();

            this.latestCopy = new Subscription[0];
        }

        public Subscription[] Subscriptions
        {
            get
            {
                return this.latestCopy;
            }
        }

        public void Add(Subscription subscription)
        {
            if (!this.subscriptions.Contains(subscription))
            {
                this.subscriptions.Add(subscription);
                this.latestCopy = this.subscriptions.ToArray();
            }
        }

        public void Remove(Subscription subscription)
        {
            if (this.subscriptions.Contains(subscription))
            {
                this.subscriptions.Remove(subscription);
                this.latestCopy = this.subscriptions.ToArray();
            }
        }
    }
}
