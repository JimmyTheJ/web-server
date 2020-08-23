using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.AspNetCore.SpaServices.Extensions;

using Newtonsoft.Json;

using System;

using VueServer.Classes.Extensions;
using VueServer.Classes;
using VueServer.Classes.Scheduling;
using VueServer.Models;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using System.Linq;
using System.IO;
using VueServer.Domain.Helper;
using Microsoft.Extensions.Hosting;
using VueServer.Services.Hubs;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace VueServer
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Environment = env;
        }

        public IConfigurationRoot Configuration { get; }

        public IWebHostEnvironment Environment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache(options => {
                options.ExpirationScanFrequency = TimeSpan.FromHours(8);
                options.SizeLimit = 256 * 1024 * 1024;
            });

            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
                hubOptions.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
            }).AddNewtonsoftJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver = new LowercaseContractResolver();
                options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddCustomSession();

            //services.AddDataProtection(options => options.ApplicationDiscriminator = "VueServer")
            //    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))
            //    .SetDefaultKeyLifetime(TimeSpan.FromDays(365))
            //    .ProtectKeysWithDpapi();

            services.AddCustomAuthentication(Configuration);
            services.AddCustomServices(Configuration);
            services.AddCustomDataStore(Configuration);

            services
                .AddMvc(options => {
                    options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
                })
                .InitializeTagHelper<FormTagHelper>((helper, context) => helper.Antiforgery = false)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new LowercaseContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddHealthChecks();
            services.AddRouting();
            services.AddControllers();

            if (Configuration.GetSection("Options").GetValue<bool>("Https"))
                services.AddCustomHttpsRedirection(Environment, Configuration);
            services.AddCustomCompression();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            FolderBuilder.CreateFolder(Path.Combine(env.ContentRootPath, "Logs"), "Startup: Error creating log file directory");

            loggerFactory.AddFile(Path.Combine("Logs", "_log-{Date}.txt"));
            ILogger logger = loggerFactory.CreateLogger("Startup");

            app.UseResponseCompression();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                //app.UseHsts();

                // Redirect non-www to www
                var rewriteOptions = new RewriteOptions();
                rewriteOptions.AddRedirectToWww();
                app.UseRewriter(rewriteOptions);
            }

            if (Configuration.GetSection("Options").GetValue<bool>("Https"))
            {
                app.UseHttpsRedirection();
            }
            else
            {
                Console.WriteLine("Https is disabled. Consider enabling it in the configuration file and providing a certificate.");
            }

            app.UseAuthentication();
            app.UseSession();
             
            // Exposes everything in the /dist folder where all our front-end files have been placed through webpack
            app.UseWebpackFiles(env, logger);

            // Exposes everything in the /public folder
            app.UsePublicFiles(env, logger);

            // Necessary for CertifyTheWeb to automatically re-authorize the webserver's TLS cert
            if (Configuration.GetSection("Options").GetValue<bool>("Well-Known"))
                app.UseAutoAuthorizerStaticFiles(env, logger);

            app.Use(async (context, next) =>
            {
                if (!context.Response.Headers.ContainsKey("Cache-Control"))
                {
                    context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate;");
                    context.Response.Headers.Add("Pragma", "no-cache");
                }

                await next();
            });

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapFallbackToController("Index", "Home");
                endpoints.MapHealthChecks("/healthcheck");
                endpoints.MapHub<ChatHub>("/chat-hub");
                endpoints.MapControllers();
            });
        }
    }
}
