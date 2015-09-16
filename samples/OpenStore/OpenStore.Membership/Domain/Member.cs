using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.Membership.Domain
{
    public class Member
    {
        public string Id { get; set; }

        public List<Login> Logins { get; set; }
        public string Email { get; set; }
    }
}
