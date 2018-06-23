using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using VethorScan.AC;
using VethorScan.AppMgr;
using VethorScan.Common;
using VethorScan.Common.CacheProfiles;
using VethorScan.Domain.Vet;

namespace VethorScan.Web
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
            services.AddSingleton<IVetSystem, VetSystem>();

            services.AddHttpClient<IVetSystem, VetSystem>(client =>
                client.BaseAddress = new Uri(Configuration["VenCoinMarketCapUri"]));

            services.AddCors();

            services.AddMemoryCache();

            // Build the intermediate service provider
            var sp = services.BuildServiceProvider();

            var calculatorManager = new CalculatorManager(sp.GetService<IVetSystem>(), sp.GetService<IMemoryCache>());

            //Task.Run(async () => await calculatorManager.Initialize().ConfigureAwait(false));

            services.AddSingleton(calculatorManager);

            services.AddMvc(options =>
            {
                options.CacheProfiles.Add(CacheProfilesEnum.Default.ToString(),
                    new CacheProfile()
                    {
                        Duration = 60
                    });
                options.CacheProfiles.Add(CacheProfilesEnum.Never.ToString(),
                    new CacheProfile()
                    {
                        Location = ResponseCacheLocation.None,
                        NoStore = true
                    });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {Title = "VethorScan API", Version = "v1"});
                c.MapType<decimal>(() => new Schema { Type = "number", Format = "decimal"});
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });
        }
    }
}
