using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Cors;
using Newtonsoft.Json.Serialization;

namespace OpenStore.Api.Configuration
{
    class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Formatters.JsonFormatter.AddUriPathExtensionMapping("json", JsonMediaTypeFormatter.DefaultMediaType);
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            config.Formatters.XmlFormatter.AddUriPathExtensionMapping("xml", XmlMediaTypeFormatter.DefaultMediaType);

            // cors
            // TODO: Should come from web.config
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);
        }
    }
}
