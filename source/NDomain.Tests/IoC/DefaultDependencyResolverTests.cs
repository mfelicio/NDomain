using NUnit.Framework;
using NDomain.Configuration;
using NDomain.Tests.Common.Specs;

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
