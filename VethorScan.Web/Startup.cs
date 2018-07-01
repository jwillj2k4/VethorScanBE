using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using VethorScan.AC;
using VethorScan.AppMgr;
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
            
            services.AddMemoryCache();

            // Build the intermediate service provider
            var sp = services.BuildServiceProvider();

            var calculatorManager = new CalculatorManager(new CalculatorService(sp.GetService<IMemoryCache>(), sp.GetService<IVetSystem>()));

            //Task.Run(async () => await calculatorManager.Initialize().ConfigureAwait(false));

            services.AddSingleton(calculatorManager);

            services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = true;
            });

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

            /*************************ADD CORS***************************************/
            /*************************ADD CORS***************************************/
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowSpecificOrigin"));
            });
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://localhost:5000").AllowAnyHeader()
                        .AllowAnyMethod());
            });
            /*************************ADD CORS***************************************/
            /*************************ADD CORS***************************************/

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {Title = "VeThor Tracker API", Version = "v1"});
                c.MapType<decimal>(() => new Schema { Type = "number", Format = "decimal"});
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            /*************************ADD CORS***************************************/
            /*************************ADD CORS***************************************/

            // Shows UseCors with named policy.
            app.UseCors("AllowSpecificOrigin");

            /*************************ADD CORS***************************************/
            /*************************ADD CORS***************************************/

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "My API V1");
            });

            app.Use(async (context, next) =>
            {
                await next.Invoke();

                //After going down the pipeline check if we 404'd. 
                if (context.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    await context.Response.WriteAsync("Woops! We 404'd");
                }
                else if(context.Response.StatusCode != StatusCodes.Status200OK)
                {
                    await context.Response.WriteAsync(": Something went terribly wrong!");
                }
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}
