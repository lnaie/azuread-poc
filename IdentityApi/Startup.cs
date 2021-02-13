
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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
                builder.WithOrigins("https://localhost:44358", "https://localhost:17443")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));

            var clientId = Configuration["AzureAd:ClientId"];
            var azureAdBearerScheme = "AzureADBearer"; //"Bearer"; 

            //services.AddMicrosoftIdentityWebApiAuthentication(
            //        Configuration,
            //        configSectionName: "AzureAd",
            //        jwtBearerScheme: azureAdBearerScheme,
            //        subscribeToJwtBearerMiddlewareDiagnosticsEvents: true
            //    );
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = Configuration["IdentityServer:Authority"];
                    options.MetadataAddress = Configuration["IdentityServer:DiscoveryAddress"];
                    options.RequireHttpsMetadata = false;

                    options.SaveToken = true;
                    options.IncludeErrorDetails = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    };
                    //options.SecurityTokenValidators.Clear();
                    //options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler
                    //{
                    //    MapInboundClaims = false
                    //});
                })
                .AddMicrosoftIdentityWebApi(Configuration,
                    configSectionName: "AzureAd",
                    jwtBearerScheme: azureAdBearerScheme,
                    subscribeToJwtBearerMiddlewareDiagnosticsEvents: true
                );
            services.AddAuthorization(options =>
            {
                options.AddPolicy("IDSUser", builder =>
                {
                    builder.AuthenticationSchemes
                        .Add(JwtBearerDefaults.AuthenticationScheme);
                    builder.RequireAuthenticatedUser()
                        .RequireClaim("client_id")
                        .RequireClaim("aud")
                        .RequireClaim("scope", "api.public")
                        .RequireClaim(ClaimTypes.NameIdentifier)
                        .RequireClaim("name")
                        //.RequireClaim(ClaimTypes.Name)
                        .RequireClaim(ClaimTypes.Email)
                        .RequireClaim(ClaimTypes.Role);
                });
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
