using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SportScraper.Data;

namespace SportScraper
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IResultWriter, XmlResultWriter>();
            services.AddSingleton<IBasicFitScraper, BasicFitScraper>();
            services.AddSingleton<ISportInformation, DiskSportInformation>();
            services.AddHttpClient();
            
            services.AddRazorPages();

            services.AddHostedService<BasicFitScraper>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
           {
               endpoints.MapRazorPages();
           });
 
        }
    }
}
