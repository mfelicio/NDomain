using Autofac;
using NDomain.Configuration;
using NDomain.Tests.Common.Specs;
using NUnit.Framework;

namespace NDomain.Autofac.Tests.IoC
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
