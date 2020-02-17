using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApiTutorial.Connectors;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
            services.AddControllers();
            
            // Dependency Injection for RestController.
            var rest_endpoint = Environment.GetEnvironmentVariable("REST_URI") ?? "https://catalog.data.gov";
            services.AddSingleton<IRestConnector>(new RestConnector(new Uri(rest_endpoint)));

            // Dependency Injection for SoapController.
            var ns = Environment.GetEnvironmentVariable("SOAP_NAMESPACE") ?? "http://tempuri.org/";
            var soap_endpoint = Environment.GetEnvironmentVariable("SOAP_ENDPOINT") ?? "https://www.gcmrc.gov/WebService.asmx";
            services.AddSingleton<ISoapConnector>(new SoapConnector(XNamespace.Get(ns), new Uri(soap_endpoint)));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("CorsPolicy");

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}