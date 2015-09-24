using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace OpenStore.Api.Configuration
{
    public class ApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            RoutesConfig.Register(config);
            WebApiConfig.Register(config);
            SwaggerConfig.Register(config);
        }
    }
}
