using NDomain.CQRS;
using OpenStore.Api.Models.Requests;
using OpenStore.Marketplace.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace OpenStore.Api.Controllers
{
    public class AuctionController : ApiController
    {
        readonly ICommandBus commandBus;

        public AuctionController(ICommandBus bus)
        {
            this.commandBus = bus;
        }

        /// <summary>
        /// Create auction
        /// </summary>
        /// <remarks>Idempotent</remarks>
        /// <param name="request">title</param>
        /// <response code="200">Ok</response>
        /// <response code="202">Accepted, check Location header</response>
        /// <returns></returns>
        [Route("auctions")]
        [HttpPost]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> CreateAuction([FromBody]CreateAuctionRequest request)
        {
            var sellerId = "the-seller-id"; // would get from an user context

            var auctionId = Guid.NewGuid().ToString(); // would get from an IdGenerator<Auction>

            var command = new Command<CreateAuction>(
                                Guid.NewGuid().ToString(),
                                new CreateAuction
                                {
                                    SellerId = sellerId,
                                    Item = request.Item,
                                    MinPrice = request.MinPrice,
                                    WindowUtc = request.WindowUtc
                                });

            await this.commandBus.Send(command);

            // TODO: change to 202 accepted with location
            return this.Ok();
        }
    }
}
