using NDomain.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace OpenStore.Api.Controllers
{
    public class SaleController : ApiController
    {
        readonly ICommandBus bus;

        public SaleController(ICommandBus bus)
        {
            this.bus = bus;
        }
    }
}
