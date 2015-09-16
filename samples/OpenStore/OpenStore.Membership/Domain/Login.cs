using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Membership.Domain
{
    public class Login
    {
        public string Id { get; set; }

        public string Provider { get; set; }
        public string ExternalId { get; set; }
        public string Email { get; set; }
    }
}
