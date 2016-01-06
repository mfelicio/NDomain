using NDomain.Bus.Transport;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Bus.Transport.Redis
{
    // TODO: document the keys structure and queueing solution in place
    public class RedisTransport : IInboundTransport, IOutboundTransport
    {
        readonly ConnectionMultiplexer connection;
        readonly string inputQueue;

        readonly string keyFormat;

        public RedisTransport(ConnectionMultiplexer connection, string prefix, string inputQueue)
        {
            this.connection = connection;
            this.inputQueue = inputQueue;

            if (prefix == null)
            {
                keyFormat = "{0}";
            }
            else
            {
                keyFormat = string.Format("{0}.{{0}}", prefix);
            }
        }

        private string GetRedisKey(string format, params object[] args)
        {
            var key = string.Format(format, args);
            return GetRedisKey(key);
        }

        private string GetRedisKey(string key)
        {
            var formatedKey = string.Format(keyFormat, key);

            return formatedKey;
        }

        public Task Send(TransportMessage message)
        {
            return SendMultiple(new TransportMessage[] { message });
        }

        public Task SendMultiple(IEnumerable<TransportMessage> messages)
        {
            var redis = this.connection.GetDatabase();

            var tr = redis.CreateTransaction();

            foreach (var message in messages)
            {
                var jsonMsg = JObject.FromObject(message).ToString();
                var outputQueue = message.Headers[MessageHeaders.Endpoint];

                var messageKey = GetRedisKey("msg:{0}", message.Id);
                var outputQueueKey = GetRedisKey(outputQueue);

                tr.StringSetAsync(messageKey, jsonMsg, TimeSpan.FromDays(7), When.NotExists);
                tr.ListLeftPushAsync(outputQueueKey, message.Id);
            }

            return tr.ExecuteAsync();
        }

        public async Task<IMessageTransaction> Receive(TimeSpan? timeout = null)
        {
            var redis = this.connection.GetDatabase();

            var transactionId = Guid.NewGuid().ToString();

            string messageId = await GetQueuedMessageId(transactionId, timeout ?? TimeSpan.FromSeconds(60));

            if (messageId != null)
            {
                var message = await GetMessage(messageId);
                if (message != null)
                {
                    var transaction = new RedisMessageTransaction(
                                            message.Item1, 
                                            message.Item2,
                                            () => CommitTransaction(transactionId, messageId),
                                            () => FailTransaction(transactionId, messageId));

                    return transaction;
                }
            }

            return null;
        }

        private async Task<string> GetQueuedMessageId(string transactionId, TimeSpan timeout)
        {
            var redis = this.connection.GetDatabase();

            var inputQueueKey = GetRedisKey(this.inputQueue);
            var transactionIdKey = GetRedisKey(transactionId);

            string id = await redis.ListRightPopLeftPushAsync(inputQueueKey, transactionIdKey);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (id == null && sw.Elapsed < timeout)
            {
                await Task.Delay(50);
                id = await redis.ListRightPopLeftPushAsync(inputQueueKey, transactionIdKey);
            }

            return id;
        }

        private async Task<Tuple<TransportMessage, int>> GetMessage(string messageId)
        {
            var redis = this.connection.GetDatabase();

            var messageKey = GetRedisKey("msg:{0}", messageId);
            var retryCountKey = GetRedisKey("msg:{0}:retry", messageId);

            var data = await redis.StringGetAsync(new RedisKey[] { messageKey, retryCountKey });

            if(data[0].IsNullOrEmpty)
            {
                return null;
            }
            
            string jsonMsg = data[0];
            int retryCount = data[1].IsNull ? 0 : int.Parse(data[1]);

            var message = JObject.Parse(jsonMsg).ToObject<TransportMessage>();
            return new Tuple<TransportMessage, int>(message, retryCount);
        }

        private async Task CommitTransaction(string transactionId, string messageId)
        {
            var redis = this.connection.GetDatabase();

            var tr = redis.CreateTransaction();

            var messageKey = GetRedisKey("msg:{0}", messageId);
            var messageRetryCountKey = GetRedisKey("msg:{0}:retry", messageId);
            var transactionIdKey = GetRedisKey(transactionId);

            tr.KeyDeleteAsync(messageKey);
            tr.KeyDeleteAsync(messageRetryCountKey);
            tr.KeyDeleteAsync(transactionIdKey);
            
            await tr.ExecuteAsync();
        }

        private async Task FailTransaction(string transactionId, string messageId)
        {
            var redis = this.connection.GetDatabase();

            var tr = redis.CreateTransaction();

            var messageRetryCountKey = GetRedisKey("msg:{0}:retry", messageId);
            var transactionIdKey = GetRedisKey(transactionId);
            var inputQueueKey = GetRedisKey(this.inputQueue);

            tr.StringIncrementAsync(messageRetryCountKey);
            tr.ListRightPopLeftPushAsync(transactionIdKey, inputQueueKey);

            await tr.ExecuteAsync();
        }
    }
}
