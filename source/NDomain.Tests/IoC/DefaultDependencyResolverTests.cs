using NDomain.Tests.Specs;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDomain.Configuration;

namespace NDomain.Tests.IoC
{
    [TestFixture]
    public class DefaultDependencyResolverTests : DependencyResolverSpecs
    {
        protected override void ConfigureIoC(IoCConfigurator ioc)
        {
            ioc.UseDefault(r => { });
        }
    }
}
