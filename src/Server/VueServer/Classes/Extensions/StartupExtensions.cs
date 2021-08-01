using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
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
using VueServer.Controllers.Filters;
using VueServer.Core.Cache;
using VueServer.Core.Helper;
using VueServer.Core.Status;
using VueServer.Domain.Enums;
using VueServer.Models.Context;
using VueServer.Models.Identity;
using VueServer.Models.User;
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
        public static void AddCustomAuthentication(this IServiceCollection services, IConfiguration config)
        {
            DatabaseTypes dbType = (DatabaseTypes)config.GetSection("Options").GetValue<int>("DatabaseType");
            if (dbType == DatabaseTypes.MSSQLSERVER)
                services.AddTransient<IWSContext, SqlServerWSContext>();
            else if (dbType == DatabaseTypes.SQLITE)
                services.AddTransient<IWSContext, SqliteWSContext>();
            else if (dbType == DatabaseTypes.MYSQL)
                services.AddTransient<IWSContext, MySqlWSContext>();

            // Setup the custom identity framework dependencies.
            services.AddTransient<IUserStore<WSUser>, ServerUserStore>();
            services.AddTransient<IRoleStore<WSRole>, ServerRoleStore>();

            services.AddIdentity<WSUser, WSRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 16;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890-._@+ ";
            });

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
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
                        ClockSkew = TimeSpan.Zero,
                    };

                    // For refresh tokens
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                context.Response.Headers.Add("Token-Expired", "true");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
        }

        /// <summary>
        /// Session settings for the application
        /// </summary>
        /// <param name="services"></param>
        public static void AddCustomSession(this IServiceCollection services)
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
        /// Add all services needed for the application
        /// </summary>
        /// <param name="services"></param>
        public static void AddCustomServices(this IServiceCollection services, IConfigurationRoot config)
        {
            services.AddSingleton(a => config);
            services.AddSingleton<IStatusCodeFactory<IActionResult>, StatusCodeFactory>();
            services.AddSingleton<IVueServerCache, VueServerCache>();

            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IDirectoryService, DirectoryService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<ILibraryService, LibraryService>();
            services.AddScoped<IWeightService, WeightService>();

            services.AddScoped<ModuleAuthFilter>();
            services.AddTransient<IUserService, UserService>();


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
        public static void AddCustomDataStore(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
        {
            ConnectionStrings.WSCONTEXT = config.GetConnectionString("WebServerDbConnectionString");

            DatabaseTypes dbType = (DatabaseTypes)config.GetSection("Options").GetValue<int>("DatabaseType");
            if (dbType == DatabaseTypes.MSSQLSERVER)
            {
                if (env.IsDevelopment())
                {
                    using (var client = new SqlServerWSContext())
                    {
                        client.Database.Migrate();
                    }
                }

                services.AddEntityFrameworkSqlServer().AddDbContext<IWSContext, SqlServerWSContext>
                    (options => options.UseSqlServer(ConnectionStrings.WSCONTEXT, a => a.MigrationsAssembly("VueServer")), ServiceLifetime.Scoped);
            }
            else if (dbType == DatabaseTypes.SQLITE)
            {
                if (env.IsDevelopment())
                {
                    using (var client = new SqliteWSContext())
                    {
                        client.Database.Migrate();
                    }
                }

                services.AddEntityFrameworkSqlite().AddDbContext<IWSContext, SqliteWSContext>
                    (options => options.UseSqlite(ConnectionStrings.WSCONTEXT, a => a.MigrationsAssembly("VueServer")), ServiceLifetime.Scoped);
            }
            else if (dbType == DatabaseTypes.MYSQL)
            {
                if (env.IsDevelopment())
                {
                    using (var client = new MySqlWSContext())
                    {
                        client.Database.Migrate();
                    }
                }

                services.AddEntityFrameworkMySql().AddDbContext<IWSContext, MySqlWSContext>
                    (options => options.UseMySql(ConnectionStrings.WSCONTEXT, a => a.MigrationsAssembly("VueServer")), ServiceLifetime.Scoped);
            }
        }

        /// <summary>
        /// HTTPS redirection settings. Seperate redirect ports and status codes based on development/production
        /// </summary>
        /// <param name="services"></param>
        /// <param name="env"></param>
        public static void AddCustomHttpsRedirection(this IServiceCollection services, IWebHostEnvironment env, IConfiguration config)
        {
            var port = config.GetSection("Options").GetValue<int>("Port");
            if (env.IsDevelopment())
            {
                services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = port;
                    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                });
            }
            else
            {
                services.AddHttpsRedirection(options =>
                {
                    options.HttpsPort = port;
                    options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
                });
            }
        }

        /// <summary>
        /// Add GZip compression to non-HTTPS requests
        /// </summary>
        /// <param name="services"></param>
        public static void AddCustomCompression(this IServiceCollection services)
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
        public static void UseWebpackFiles(this IApplicationBuilder app, IWebHostEnvironment env, ILogger logger)
        {
            if (!FolderBuilder.CreateFolder(Path.Combine(env.WebRootPath, @"js"), "StartupExtensions: Error creating js folder")) return;
            if (!FolderBuilder.CreateFolder(Path.Combine(env.WebRootPath, @"css"), "StartupExtensions: Error creating css folder")) return;
            if (!FolderBuilder.CreateFolder(Path.Combine(env.WebRootPath, @"fonts"), "StartupExtensions: Error creating fonts folder")) return;

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, @"css")),
                RequestPath = new PathString("/css"),
                OnPrepareResponse = s =>
                {
                    s.Context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(30)
                    };
                }
            });

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, @"js")),
                RequestPath = new PathString("/js"),
                OnPrepareResponse = s =>
                {
                    s.Context.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(30)
                    };
                }
            });

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, @"fonts")),
                RequestPath = new PathString("/fonts"),
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
        /// Serve the /public folder. This folder contains all public files from users
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public static void UsePublicFiles(this IApplicationBuilder app, IWebHostEnvironment env, ILogger logger)
        {
            if (!FolderBuilder.CreateFolder(Path.Combine(env.WebRootPath, @"public"), "StartupExtensions: Error creating dist folder")) return;

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, @"public")),
                RequestPath = new PathString("/public"),
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
        /// This is used to expose the files necessary for TLS Certificate authentication from CertifyTheWeb
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public static void UseAutoAuthorizerStaticFiles(this IApplicationBuilder app, IWebHostEnvironment env, ILogger logger)
        {
            if (!FolderBuilder.CreateFolder(Path.Combine(env.WebRootPath, @".well-known"), "StartupExtensions: Error creating auto authorizer folder")) return;

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(env.WebRootPath, @".well-known")),
                RequestPath = new PathString("/.well-known"),
                ServeUnknownFileTypes = true,
                OnPrepareResponse = s =>
                {
                    logger.LogInformation($"Someone trying to access .well-known from: {s.Context.Connection.RemoteIpAddress.ToString()}");
                }
            });
        }
    }
}
