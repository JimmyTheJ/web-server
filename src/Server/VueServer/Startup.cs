using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using VueServer.Classes.Extensions;
using VueServer.Core.Helper;
using VueServer.Modules.Core;
using VueServer.Modules.Core.Cache;

namespace VueServer
{
    public class Startup
    {
        private const string CORS_POLICY_NAME = "front-end-server";

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
            services.AddDistributedMemoryCache(options =>
            {
                options.ExpirationScanFrequency = TimeSpan.FromHours(8);
                options.SizeLimit = 256 * 1024 * 1024;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            });
            services.AddCustomSession();

            //services.AddDataProtection(options => options.ApplicationDiscriminator = "VueServer")
            //    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))
            //    .SetDefaultKeyLifetime(TimeSpan.FromDays(365))
            //    .ProtectKeysWithDpapi();

            services.AddCustomAuthentication(Configuration);
            services.AddCustomServices(Configuration);
            services.AddCustomDataStore(Configuration, Environment);

            services
                .AddMvc(options =>
                {
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

            if (Environment.IsDevelopment())
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(name: CORS_POLICY_NAME,
                                      builder =>
                                      {
                                          builder.WithOrigins("http://localhost:8080", "https://localhost:8080")
                                            .AllowAnyMethod()
                                            .AllowAnyHeader()
                                            .AllowCredentials();
                                      });
                });
            }

            if (Configuration.GetSection("Options").GetValue<bool>("Https"))
            {
                services.AddCustomHttpsRedirection(Environment, Configuration);
            }

            services.AddCustomCompression();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies.OrderByDescending(x => x.FullName))
            {
                if (!ValidModuleAssembly(assembly))
                {
                    continue;
                }

                var startupClass = assembly.GetTypes().Where(x => x.IsClass && x.Name == "Startup").FirstOrDefault();
                if (startupClass != null)
                {
                    ConstructorInfo ctor = startupClass.GetConstructor(Type.EmptyTypes);
                    object startupClassObject = ctor.Invoke(new object[] { });

                    MethodInfo method = startupClass.GetMethod("Load");
                    object result = method.Invoke(startupClassObject, new object[] { services, Configuration, Environment });
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IVueServerCache serverCache)
        {
            FolderBuilder.CreateFolder(Path.Combine(env.ContentRootPath, "Logs"), "Startup: Error creating log file directory");

            loggerFactory.AddFile(Path.Combine("Logs", "_log-{Date}.txt"));
            ILogger logger = loggerFactory.CreateLogger("Startup");

            logger.LogInformation("Startup begin");

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

            if (string.IsNullOrWhiteSpace(env.WebRootPath))
            {
                env.WebRootPath = Path.Combine(env.ContentRootPath, "wwwroot");
            }

            // Ensure wwwroot and tmp folder exist
            FolderBuilder.CreateFolder(Path.Combine(env.WebRootPath));
            FolderBuilder.CreateFolder(Path.Combine(env.WebRootPath, "tmp"));

            // Exposes everything in the /dist folder where all our front-end files have been placed through webpack
            app.UseWebpackFiles(env, logger);

            // Exposes everything in the /public folder
            app.UsePublicFiles(env, logger);

            if (Environment.IsDevelopment())
            {
                app.UseCors(CORS_POLICY_NAME);
            }

            // Necessary for CertifyTheWeb to automatically re-authorize the webserver's TLS cert
            if (Configuration.GetSection("Options").GetValue<bool>("Well-Known"))
            {
                app.UseAutoAuthorizerStaticFiles(env, logger);
            }

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
                endpoints.MapControllers();
            });

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies.OrderByDescending(x => x.FullName))
            {
                if (!ValidModuleAssembly(assembly))
                {
                    continue;
                }

                var startupClass = assembly.GetTypes().Where(x => x.IsClass && x.Name == "Startup").FirstOrDefault();
                if (startupClass != null)
                {
                    ConstructorInfo ctor = startupClass.GetConstructor(Type.EmptyTypes);
                    object startupClassObject = ctor.Invoke(new object[] { });

                    MethodInfo method = startupClass.GetMethod("Create");
                    object result = method.Invoke(startupClassObject, new object[] { app });
                }
            }

            BuildCache(serverCache);

            logger.LogInformation("Startup complete");
        }

        private void BuildCache(IVueServerCache serverCache)
        {
            serverCache.Update(CacheMap.BlockedIP);
            serverCache.Update(CacheMap.Users);
            serverCache.Update(CacheMap.UserModuleAddOn);
            serverCache.Update(CacheMap.UserModuleFeature);
            serverCache.Update(CacheMap.LoadedModules);
        }

        private bool ValidModuleAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            if (!name.StartsWith("VueServer.Modules"))
            {
                return false;
            }

            if (name == "VueServer.Modules.Core")
            {
                return false;
            }

            return true;
        }
    }
}
