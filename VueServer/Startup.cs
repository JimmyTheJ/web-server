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

namespace VueServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
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

        public IHostingEnvironment Environment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache(options => {
                options.ExpirationScanFrequency = TimeSpan.FromHours(8);
                options.SizeLimit = 256 * 1024 * 1024;
            });

            services.AddCustomSession();
            services.AddCustomAntiforgery();

            //services.AddDataProtection(options => options.ApplicationDiscriminator = "VueServer")
            //    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))
            //    .SetDefaultKeyLifetime(TimeSpan.FromDays(365))
            //    .ProtectKeysWithDpapi();

            services.AddCustomAuthentication(Configuration);
            services.AddCustomServices(Configuration);
            services.AddCustomDataStore(Configuration);

            services
                .AddMvc(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
                .AddJsonOptions(options =>
                 {
                     options.SerializerSettings.ContractResolver = new LowercaseContractResolver();
                     options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                 });

            services.AddCustomHttpsRedirection(Environment);
            services.AddCustomCompression();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (!Directory.Exists(Path.Combine(env.ContentRootPath, "Logs") ) )
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(env.ContentRootPath, "Logs"));
                }
                catch (Exception)
                {
                    Console.WriteLine("Startup: Error creating log file directory.");
                }
            }

            loggerFactory.AddFile(Path.Combine("Logs", "_log-{Date}.txt"));
            ILogger logger = loggerFactory.CreateLogger("Startup");

            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                });
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
            app.UseWebpackFileServer(env, logger);

            // Exposes everything in /videos folder for serving video files
            // TODO: Make this optional. Have a settings in appsettings that can disable this.
            // Some callback to the front-end to know to not expose the path for /videos would be useful if disabled.
            app.UseVideoFileServer(env, logger);

            // Necessary for CertifyTheWeb to automatically re-authorize the webserver's TLS cert
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

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}
