using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Configuration
{
    public abstract class Configurator
    {
        protected Configurator(ContextBuilder builder)
        {
            this.Builder = builder;
        }

        public ContextBuilder Builder { get; private set; }
    }
}
