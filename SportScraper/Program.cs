using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace SportScraper
{
    class Program
    {
        static void Main(string[] args)
        {            
            Console.WriteLine("Sport scraper running");
            CreateHost(args).Build().Run();
        }

        public static IHostBuilder CreateHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IResultWriter, XmlResultWriter>();
                    services.AddHttpClient();
                    services.AddHostedService<BasicFitScraper>();
                });
        }
    }
}
