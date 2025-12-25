using System.Text;
using GetSense.API.Exceptions;
using GateSense.Application.Auth.Interfaces;
using GateSense.Application.Auth.Services;
using GateSense.Application.Access.Interfaces;
using GateSense.Application.Access.Services;
using GateSense.Application.Gates.Interfaces;
using GateSense.Application.Gates.Services;
using GateSense.Application.Devices.Interfaces;
using GateSense.Application.Devices.Services;
using GateSense.Application.Garages.Interfaces;
using GateSense.Application.Garages.Services;
using GateSense.Application.IoT.Interfaces;
using GateSense.Application.IoT.Services;
using GateSense.Application.Sensors.Interfaces;
using GateSense.Application.Sensors.Services;
using GateSense.Application.Logs.Interfaces;
using GateSense.Application.Logs.Services;
using GateSense.Application.Admin.Interfaces;
using GateSense.Application.Admin.Services;
using Domain.Models.Auth;
using Domain.Models.Devices;
using Domain.Models.Garages;
using Domain.Models.Gates;
using Domain.Models.Sensors;
using Infrastructure.Common;
using Infrastructure.Common.Cookies;
using Infrastructure.Common.JWT;
using Infrastructure.Common.Mappers.Auth;
using Infrastructure.Data;
using Infrastructure.Data.UnitOfWork;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Repository.Services;
using Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace GetSense.API.Configurations;

public static class ConfigureBuilder
{
    public static void Configure(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var jwtConfig = builder.Configuration.GetSection("AudienceTokenConfig").Get<AudienceTokenConfig>();

        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddSerilog(s =>
            s.ReadFrom.Configuration(builder.Configuration));

        services.AddDbContext<ApplicationDbContext>(opts =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DataContext");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                if (!string.IsNullOrEmpty(databaseUrl))
                {
                    connectionString = ConvertRailwayDatabaseUrl(databaseUrl);
                }
            }
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("ConnectionString 'DataContext' is not configured. Please set ConnectionStrings__DataContext environment variable or DATABASE_URL.");
            }
            
            opts.UseNpgsql(connectionString);
        });

        services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 3;

                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

                //turned off for development
                //options.SignIn.RequireConfirmedEmail = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddDefaultTokenProviders()
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = true;
            });

        services.AddEndpointsApiExplorer();
        AddSwagger(builder);

        services.AddProblemDetails();

        services.AddAuthorization();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.JwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtConfig.JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtConfig.JwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                o.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        var request = context.Request;
                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("JwtBearer");
                        
                        // JWT Bearer middleware automatically reads from Authorization header by default
                        // Only add fallback to cookies if token is not already set
                        if (string.IsNullOrEmpty(context.Token) && request != null && request.Cookies != null && request.Cookies.TryGetValue(AppConstants.AccessTokenCookie, out var accessToken) && !string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                            logger.LogInformation("Token extracted from cookie (fallback)");
                        }
                        else if (!string.IsNullOrEmpty(context.Token))
                        {
                            logger.LogInformation("Token found in Authorization header");
                        }
                        else
                        {
                            logger.LogWarning("No token found in Authorization header or cookies");
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("JwtBearer");
                        logger.LogInformation("JWT Token validated successfully");
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        // Log authentication failures for debugging
                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("JwtBearer");
                        logger.LogError(context.Exception, "JWT Authentication failed: {Error}", context.Exception?.Message);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger("JwtBearer");
                        logger.LogWarning("JWT Challenge triggered. Error: {Error}", context.Error);
                        return Task.CompletedTask;
                    }
                };
            });


        services.AddHttpContextAccessor();
        services.AddAutoMapper(cfg => cfg.AddProfile<AuthMappingProfile>());
        services.AddScoped<ICookieService, CookieService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAccessService, AccessService>();
        services.AddScoped<IGateService, GateService>();
        services.AddScoped<IGarageService, GarageService>();
        services.AddScoped<IGarageDeviceService, GarageDeviceService>();
        services.AddScoped<IIoTService, IoTService>();
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped(typeof(Infrastructure.Repository.Interfaces.IGenericRepository<Domain.Models.Auth.AccessToken>), typeof(Infrastructure.Repository.Services.GenericRepository<Domain.Models.Auth.AccessToken>));
        services.AddScoped(typeof(Infrastructure.Repository.Interfaces.IGenericRepository<Domain.Models.Auth.RefreshToken>), typeof(Infrastructure.Repository.Services.GenericRepository<Domain.Models.Auth.RefreshToken>));
        services.AddScoped(typeof(Infrastructure.Repository.Interfaces.IGenericRepository<GarageAccess>), typeof(Infrastructure.Repository.Services.GenericRepository<GarageAccess>));
        services.AddScoped(typeof(Infrastructure.Repository.Interfaces.IGenericRepository<AccessKey>), typeof(Infrastructure.Repository.Services.GenericRepository<AccessKey>));
        services.AddScoped(typeof(Infrastructure.Repository.Interfaces.IGenericRepository<Garage>), typeof(Infrastructure.Repository.Services.GenericRepository<Garage>));
        services.AddScoped(typeof(Infrastructure.Repository.Interfaces.IGenericRepository<IoTDevice>), typeof(Infrastructure.Repository.Services.GenericRepository<IoTDevice>));
        services.AddScoped(typeof(Infrastructure.Repository.Interfaces.IGenericRepository<GateEvent>), typeof(Infrastructure.Repository.Services.GenericRepository<GateEvent>));
        services.AddScoped(typeof(Infrastructure.Repository.Interfaces.IGenericRepository<SensorReading>), typeof(Infrastructure.Repository.Services.GenericRepository<SensorReading>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Background Services
        services.Configure<HeartbeatMonitorOptions>(
            builder.Configuration.GetSection(HeartbeatMonitorOptions.SectionName));
        services.AddHostedService<DeviceHeartbeatMonitorService>();
    }

    private static void AddSwagger(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Lexi API", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }

    private static string ConvertRailwayDatabaseUrl(string databaseUrl)
    {
        if (string.IsNullOrEmpty(databaseUrl))
            return string.Empty;

        if (databaseUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
            databaseUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(databaseUrl);
            var userInfo = uri.UserInfo.Split(':');
            var username = Uri.UnescapeDataString(userInfo[0]);
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');

            return $"Host={host};Port={port};Database={database};Username={username};Password={password};TcpKeepAlive=True;SSL Mode=Require;Trust Server Certificate=true";
        }

        return databaseUrl;
    }
}
