using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.CQRS
{
    public class CqrsMessageHeaders
    {
        public const string AggregateId = "ndomain.cqrs.aggregateId";
        public const string SequenceId = "ndomain.cqrs.sequenceId";
        public const string DateUtc = "ndomain.cqrs.dateUtc";
    }
}
