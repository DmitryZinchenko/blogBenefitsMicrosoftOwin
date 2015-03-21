using System;
using Microsoft.Owin.Hosting;

namespace SelfHost
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:5000/api";

            using (WebApp.Start<Middlewares.Startup>(url))
            {
                Console.WriteLine("server is listening on {0}", url);
                Console.ReadLine();
            }
        }
    }
}
