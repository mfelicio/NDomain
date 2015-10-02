using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenStore.SingleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseAddress = "http://localhost:8081";

            using (WebApp.Start<App>(url: baseAddress))
            {
                Console.WriteLine("Press ENTER to exit");
                Console.ReadLine();
            }
        }
    }
}
