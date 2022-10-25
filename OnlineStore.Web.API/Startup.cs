using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineStore.Data.Providers.Context;
using OnlineStore.Web.API.Services;
using Newtonsoft.Json;
using OnlineStore.Web.API.Configuration;
using OnlineStore.Web.API.Configuration.Middleware;
using OnlineStore.Web.API.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using OnlineStore.Web.API.Configuration.Options;

namespace OnlineStore.Web.API
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
            services.AddControllers().AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                opt.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.ConfigureAuth(Configuration);

            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;

                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);

                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("api-version"),
                    new MediaTypeApiVersionReader("version"),
                    new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader("api-version")
                );
            });

            services.AddVersionedApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVV";
                setup.SubstituteApiVersionInUrl = true;
            });

            services.AddSwaggerGen();
            services.ConfigureOptions<ConfigureSwaggerOptions>();

            services.AddScoped<HeaderMiddleware>();

            services.AddHostedService<DatabaseInitilizerHostedService>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:85", "http://localhost:8080")
                        .AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("*");
                });
            });

            services.AddDirectoryBrowser();

            services.AddHealthChecks()
                .AddDbContextCheck<OnlineStoreContext>("DbConnectionCheck", tags: new[] { "database" })
                .AddCheck<MemoryHealthCheck>("memory_check", HealthStatus.Degraded)
                .AddTypeActivatedCheck<RandomHealthCheck>("RandomCheck", failureStatus: HealthStatus.Degraded, tags: new[] { "random" });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors();

            app.UseHeader();
            app.UseIpAdresses();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    //options.SwaggerEndpoint($"http://localhost:5000/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    //options.RoutePrefix = string.Empty;
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.yaml", description.GroupName.ToUpperInvariant());
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/db", new HealthCheckOptions
                {
                    Predicate = (check) => check.Tags.Contains("database") && check.Name == "DbConnectionCheck",
                    ResponseWriter = HealthCheckWriteReposne
                });

                endpoints.MapHealthChecks("/health/random", new HealthCheckOptions
                {
                    Predicate = (check) => check.Name == "RandomCheck",
                    ResponseWriter = HealthCheckWriteReposne
                });

                endpoints.MapHealthChecks("/health/memory", new HealthCheckOptions
                {
                    Predicate = (check) => check.Name == "memory_check",
                    ResponseWriter = HealthCheckWriteReposne
                });

                endpoints.MapControllers();
            });
        }


        private static Task HealthCheckWriteReposne(HttpContext context, HealthReport result)
        {
            context.Response.ContentType = "application/json";

            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(pair =>
                    new JProperty(pair.Key, new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("description", pair.Value.Description),
                        new JProperty("data", new JObject(pair.Value.Data.Select(
                            p => new JProperty(p.Key, p.Value))))))))));

            return context.Response.WriteAsync(
                json.ToString(Formatting.Indented));
        }
    }
}
