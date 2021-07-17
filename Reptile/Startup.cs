using System;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Service.Implements;
using Service.Interfaces;

namespace Reptile
{
    /// <summary>
    /// Startup
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Startup
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// ConfigureServices
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddControllers();

            services.AddControllers(options =>
            {
                options.OutputFormatters.RemoveType<XmlDataContractSerializerOutputFormatter>();
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            }).AddNewtonsoftJson();

            services.AddScoped<INbaPlayerService, NbaPlayerService>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(name: "v1", new OpenApiInfo
                {
                    Title = "ASP.NET Core 3.1 Reptile",
                    Version = "v1"
                });

                //Set the comments path for the Swagger JSON and UI
                var basePath = AppContext.BaseDirectory;
                var xmlFiles = Directory.EnumerateFiles(basePath, searchPattern: "*.xml", SearchOption.TopDirectoryOnly);

                foreach (var xmlFile in xmlFiles)
                {
                    options.IncludeXmlComments(xmlFile);
                }
            });
        }

        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // ±j¨î https
            app.Use(async (context, next) =>
            {
                context.Request.Scheme = "https";
                await next();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors("CorsPolicy");

            app.UseRouting();

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(url: "/swagger/v1/swagger.json", name: "Reptile.WebService V1");
            });

            app.UseReDoc(options =>
            {
                options.SpecUrl("/swagger/v1/swagger.json");
                options.RoutePrefix = "redoc";
                options.DocumentTitle = "Reptile.WebService V1";
            });

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}