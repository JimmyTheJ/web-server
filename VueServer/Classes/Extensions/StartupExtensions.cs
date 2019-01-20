using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VueServer.Classes.Scheduling;
using VueServer.Common.Enums;
using VueServer.Common.Factory.Concrete;
using VueServer.Common.Factory.Interface;
using VueServer.Models;
using VueServer.Models.Account;
using VueServer.Models.Context;
using VueServer.Models.Directory;
using VueServer.Services.Concrete;
using VueServer.Services.Interface;

namespace VueServer.Classes.Extensions
{
    public static class StartupExtensions
    {
        /// <summary>
        /// Custom authentication for the application. JWT and cookie authentication included.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        public static void AddCustomAuthentication (this IServiceCollection services, IConfiguration config)
        {
            services.AddIdentity<ServerIdentity, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 1;
            }).AddEntityFrameworkStores<UserContext>().AddDefaultTokenProviders();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options => {
                    // First autorization takes priority unless explicitly targetted
                    options.RequireHttpsMetadata = true;       // TODO - Change for production
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SigningKey"])),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    // For refresh tokens
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException) )
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                })
                // TODO: See if a cookie is actually necessary - Might be for file downloads
                .AddCookie(options =>
                {
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.Expiration = TimeSpan.FromDays(365);
                    options.Cookie.Name = "WebServer.user";
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Events.OnRedirectToReturnUrl = context =>
                    {
                        Console.WriteLine("Authentication Cookie: redirect to return url");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToAccessDenied = context =>
                    {
                        Console.WriteLine("Authentication Cookie: redirect on accest denied");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToLogin = context =>
                    {
                        Console.WriteLine("Authentication Cookie: redirect to login");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                    options.Events.OnRedirectToLogout = context =>
                    {
                        Console.WriteLine("Authentication Cookie: redirect to logout");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    };
                });
        }

        /// <summary>
        /// Session settings for the application
        /// </summary>
        /// <param name="services"></param>
        public static void AddCustomSession (this IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.Cookie.Name = "WebServer.session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.IdleTimeout = TimeSpan.FromDays(365);
            });
        }

        /// <summary>
        /// Anti-forgery settings for the application
        /// </summary>
        /// <param name="services"></param>
        public static void AddCustomAntiforgery(this IServiceCollection services)
        {
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "WebServer.anti-forgery";
                options.Cookie.HttpOnly = false;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                options.Cookie.Expiration = TimeSpan.FromDays(1);
                options.FormFieldName = "AntiForgery";
                options.HeaderName = "X-CSRF-TOKEN";
                options.SuppressXFrameOptionsHeader = true;
            });
        }

        /// <summary>
        /// Add all services needed for the application
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        public static void AddCustomServices (this IServiceCollection services, IConfigurationRoot config)
        {
            services.AddSingleton(a => config);
            services.AddSingleton<IStatusCodeFactory<IActionResult>, StatusCodeFactory>();

            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUploadService, UploadService>();
            services.AddScoped<IDirectoryService, DirectoryService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<IFileServerService, FileServerService>();

            services.AddScoped<IUserService, UserService>();

            // Scheduled task for deleting the files from the temporary folder
            services.AddSingleton<IScheduledTask, TempFileDeletionTask>();
            services.AddScheduler((sender, args) =>
            {
                Console.Write(args.Exception.Message);
                args.SetObserved();
            });
        }

        /// <summary>
        /// Connection strings and database setup for the application
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param>
        public static void AddCustomDataStore (this IServiceCollection services, IConfiguration config)
        {
            string userConnectionString = config.GetConnectionString("UserDbConnectionString");
            string webServerConnectionString = config.GetConnectionString("WebServerDbConnectionString");

            DatabaseTypes dbType = (DatabaseTypes) config.GetValue<int>("Options:DatabaseType");
            if (dbType == DatabaseTypes.MSSQLSERVER)
            {
                using (var client = new UserContext(new DbContextOptionsBuilder<UserContext>().UseSqlServer(userConnectionString, a => a.MigrationsAssembly("VueServer")).Options))
                {
                    client.Database.EnsureCreated();
                }

                using (var client = new WSContext(new DbContextOptionsBuilder<WSContext>().UseSqlServer(webServerConnectionString, a => a.MigrationsAssembly("VueServer")).Options))
                {
                    client.Database.Migrate();
                }

                services.AddEntityFrameworkSqlServer().AddDbContext<UserContext>
                    (options => options.UseSqlServer(userConnectionString, a => a.MigrationsAssembly("VueServer")), ServiceLifetime.Scoped);

                services.AddEntityFrameworkSqlServer().AddDbContext<WSContext>
                    (options => options.UseSqlServer(webServerConnectionString, a => a.MigrationsAssembly("VueServer")), ServiceLifetime.Scoped);
            }
            else if (dbType == DatabaseTypes.SQLITE)
            {
                using (var client = new UserContext(new DbContextOptionsBuilder<UserContext>().UseSqlite(userConnectionString, a => a.MigrationsAssembly("VueServer")).Options))
                {
                    client.Database.EnsureCreated();
                }

                using (var client = new WSContext(new DbContextOptionsBuilder<WSContext>().UseSqlite(webServerConnectionString, a => a.MigrationsAssembly("VueServer")).Options))
                {
                    client.Database.Migrate();
                }

                services.AddEntityFrameworkSqlite().AddDbContext<UserContext>
                    (options => options.UseSqlite(userConnectionString, a => a.MigrationsAssembly("VueServer")), ServiceLifetime.Scoped);

                services.AddEntityFrameworkSqlite().AddDbContext<WSContext>
                    (options => options.UseSqlite(webServerConnectionString, a => a.MigrationsAssembly("VueServer")), ServiceLifetime.Scoped);
            }
        }

        /// <summary>
        /// HTTPS redirection settings. Seperate redirect ports and status codes based on development/production
        /// </summary>
        /// <param name="services"></param>
        /// <param name="env"></param>
        public static void AddCustomHttpsRedirection(this IServiceCollection services, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = 7757;
                    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                });
            }
            else
            {
                services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = 443;   // Set this in config file
                    options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
                });
            }
        }

        /// <summary>
        /// Add GZip compression to non-HTTPS requests
        /// </summary>
        /// <param name="services"></param>
        public static void AddCustomCompression (this IServiceCollection services)
        {
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest;
            });
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
                options.EnableForHttps = false;
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
            });
        }

        /// <summary>
        /// Serve the /dist folder. This folder contains all the contents of the base files for the SPA
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public static void UseWebpackFileServer (this IApplicationBuilder app, IHostingEnvironment env, ILogger logger)
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, @"dist")),
                RequestPath = new PathString("/dist"),
                OnPrepareResponse = s =>
                {
                    s.Context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(30)
                    };
                }
            });
        }

        /// <summary>
        /// Serve files for video streaming in the /videos folder of wwwroot
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public static void UseVideoFileServer(this IApplicationBuilder app, IHostingEnvironment env, ILogger logger)
        {
            //var provider = new FileExtensionContentTypeProvider();
            //provider.Mappings[".mkv"] = "video/webm";
            //provider.Mappings[".mkv"] = "video/x-matroska";

            // Video file serving
            var options = new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, @"videos")),
                RequestPath = new PathString("/videos"),
                EnableDirectoryBrowsing = true,
                EnableDefaultFiles = true
            };
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            //options.StaticFileOptions.ContentTypeProvider = provider;
            //options.StaticFileOptions.DefaultContentType = "video/webm";
            options.StaticFileOptions.OnPrepareResponse = s =>
            {
                //s.Context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate;");
                //s.Context.Response.Headers.Add("Pragma", "no-cache");
                Console.WriteLine("Attempting to download video file!");
            };
            app.UseFileServer(options);
        }

        /// <summary>
        /// This is used to expose the files necessary for TLS Certificate authentication from CertifyTheWeb
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public static void UseAutoAuthorizerStaticFiles(this IApplicationBuilder app, IHostingEnvironment env, ILogger logger)
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, @".well-known")),
                RequestPath = new PathString("/.well-known"),
                ServeUnknownFileTypes = true,
                OnPrepareResponse = s =>
                {
                    logger.LogInformation("Someone trying to access .well-known folder from: " + s.Context.Connection.RemoteIpAddress.ToString());
                }
            });
        }
    }
}
