using NUnit.Framework;
using NDomain.Configuration;
using Autofac;
using NDomain.Tests.Common.Specs;

namespace NDomain.Autofac.Tests
{
    [TestFixture]
    public class AutofacDependencyResolverShould : DependencyResolverSpecs
    {
        private IContainer container;

        [SetUp]
        public void SetUp()
        {
            this.container = new ContainerBuilder().Build();
        }

        [TearDown]
        public void TearDown()
        {
            this.container.Dispose();
        }

        protected override void ConfigureIoC(IoCConfigurator ioc)
        {
            ioc.WithAutofac(this.container);
        }
    }
}
