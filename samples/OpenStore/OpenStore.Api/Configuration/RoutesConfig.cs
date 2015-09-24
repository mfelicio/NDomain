using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace OpenStore.Api.Configuration
{
    class RoutesConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // attribute routes
            config.MapHttpAttributeRoutes();

            // conventional routes
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
