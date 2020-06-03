using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using Winleafs.Server.Api.Configuration;
using Winleafs.Server.Data;

namespace Winleafs.Server.Api
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
            ConfigureCors(services);
            ConfigureMvc(services);
            ConfigureSwagger(services);
            services
                .AddPatterns()
                .AddServicesAndRepositories();

            services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            ConfigureMvc(app, env);
            ConfigureCors(app);
            ConfigureSwagger(app);
        }

        #region Mvc

        private static void ConfigureMvc(IServiceCollection services)
        {
            services.AddMvc(option => option.EnableEndpointRouting = false);
        }

        private static void ConfigureMvc(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc();
        }

        #endregion

        #region Cors

        private static void ConfigureCors(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://localhost:4200")
                        .AllowAnyMethod()
                        .AllowAnyHeader().AllowCredentials());
            });
        }

        private static void ConfigureCors(IApplicationBuilder application)
        {
            application.UseCors("AllowSpecificOrigin");
        }

        #endregion

        #region Swagger

        private static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = $"Winleafs API", Version = "v1"});
                options.IncludeXmlComments($@"{AppDomain.CurrentDomain.BaseDirectory}Winleafs.Server.Api.xml");
                options.DescribeAllEnumsAsStrings();
            });
        }

        private static void ConfigureSwagger(IApplicationBuilder application)
        {
            application
                .UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Template API v1");
                    c.DocExpansion(DocExpansion.None);
                    c.RoutePrefix = string.Empty;
                    c.DisplayRequestDuration();
                });
        }

        #endregion
    }
}