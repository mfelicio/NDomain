using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Configuration
{
    /// <summary>
    /// Base class for all configurators. Provides access to the ContextBuilder
    /// </summary>
    public abstract class Configurator
    {
        protected Configurator(ContextBuilder builder)
        {
            this.Builder = builder;
        }

        /// <summary>
        /// ContextBuilder that contains the state of the build configurators used to build the current DomainContext
        /// </summary>
        public ContextBuilder Builder { get; private set; }
    }
}
