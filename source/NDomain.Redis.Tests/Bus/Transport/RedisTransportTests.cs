using NDomain.Bus.Transport;
using NDomain.Bus.Transport.Redis;
using NDomain.Tests.Bus.Transport;
using NDomain.Tests.Specs;
using NUnit.Framework;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Redis.Tests.Bus.Transport
{
    [TestFixture(Category = "Redis")]
    public class RedisTransportTests : TransportSpecs
    {
        static readonly string serverEndpoint = "localhost:6379";

        ConnectionMultiplexer connection;

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            var options = new ConfigurationOptions();
            options.AllowAdmin = true;
            options.EndPoints.Add(serverEndpoint);

            try
            {
                this.connection = ConnectionMultiplexer.Connect(options);
                this.connection.PreserveAsyncOrder = false;
            }
            catch (Exception ex)
            {
                Assert.Inconclusive(ex.Message);
            }
        }

        public override ITransportFactory CreateFactory()
        {
            return new RedisTransportFactory(connection, "ndomain.redis.tests");
        }

        protected override void OnSetUp()
        {
            Clear();
        }

        protected override void OnTearDown()
        {
            Clear();
        }

        private void Clear()
        {
            var keys = connection.GetServer(serverEndpoint)
                                 .Keys(pattern: "ndomain.redis.tests*");

            var redis = connection.GetDatabase();
            foreach (var key in keys)
            {
                redis.KeyDelete(key);
            }
        }
    }
}
