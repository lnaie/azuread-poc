// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace TodoListService
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
            services.AddCors(options => options.AddPolicy("MDOrigins", builder =>
            {
                builder.WithOrigins("https://localhost:44358")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));

            var clientId = Configuration["AzureAd:ClientId"];
            var azureAdBearerScheme = "AzureADBearer"; //"Bearer"; 

            services.AddMicrosoftIdentityWebApiAuthentication(
                    Configuration,
                    configSectionName: "AzureAd",
                    jwtBearerScheme: azureAdBearerScheme,
                    subscribeToJwtBearerMiddlewareDiagnosticsEvents: true
                );
            //services.AddAuthentication()
            //    .AddMicrosoftIdentityWebApi(Configuration,
            //        configSectionName: "AzureAd",
            //        jwtBearerScheme: azureAdBearerScheme,
            //        subscribeToJwtBearerMiddlewareDiagnosticsEvents: true
            //    )
            //    .EnableTokenAcquisitionToCallDownstreamApi()
            //    .AddInMemoryTokenCaches();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AzureADUser", policy =>
                {
                    policy.AuthenticationSchemes.Add(azureAdBearerScheme);
                    policy
                        .RequireAuthenticatedUser()
                        //.RequireClaim("appid", clientId)
                        .RequireClaim("aud", $"api://{clientId}")
                        .RequireClaim("http://schemas.microsoft.com/identity/claims/scope", "api.access")
                        .RequireClaim(ClaimTypes.NameIdentifier)
                        .RequireClaim(ClaimTypes.Role);
                });
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("MDOrigins");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
