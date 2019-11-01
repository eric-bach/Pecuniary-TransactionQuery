using System;
using System.Reflection;
using EricBach.CQRS.QueryRepository;
using EricBach.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pecuniary.Transaction.Data.Models;

namespace Pecuniary.Transaction.Query
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            RegisterServices(services);

            // Add MediatR
            services.AddMediatorHandlers(typeof(Startup).GetTypeInfo().Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            // ASP.NET HttpContext dependency
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Query Service
            //services.AddScoped<IReadRepository<AccountViewModel>, InMemoryRepository>();
            services.AddScoped<IReadRepository<TransactionReadModel>>(r =>
                new ElasticSearchRepository<TransactionReadModel>(Environment.GetEnvironmentVariable("ElasticSearchDomain")));
        }
    }
}
