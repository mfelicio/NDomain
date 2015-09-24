using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace OpenStore.Api.Configuration
{
    class SwaggerConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config
                .EnableSwagger(c =>
                {
                    c.SingleApiVersion("1.0", "OpenStore Http API")
                     .Description("OpenStore Http API from NDomain samples")
                     .Contact(cb => 
                         cb.Name("NDomain")
                           .Url("https://github.com/mfelicio/ndomain"))
                     .License(l => 
                         l.Name("MIT")
                          .Url("http://opensource.org/licenses/mit-license.php"));

                    c.IncludeXmlComments("OpenStore.Api.XML");
                })
                .EnableSwaggerUi();
        }
    }
}
