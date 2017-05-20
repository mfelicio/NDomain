using NDomain.CQRS;
using NDomain.Model.EventSourcing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
