using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS.Projections
{
    /// <summary>
    /// [Experimental] Represents a query object where a read model is stored
    /// </summary>
    public class Query
    {
        protected Query()
        {

        }

        public string Id { get; set; }
        public int Version { get; set; }
        public DateTime DateUtc { get; set; }

        public object Data { get; set; }
    }

    public class Query<T> : Query
    {
        public new T Data
        {
            get { return (T)base.Data; }
            set { base.Data = value; }
        }
    }
}
