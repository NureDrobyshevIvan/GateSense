using System.Text;
using GetSense.API.Exceptions;
using GateSense.Application.Auth.Interfaces;
using GateSense.Application.Auth.Services;
using Domain.Models.Auth;
using Infrastructure.Common;
using Infrastructure.Common.Cookies;
using Infrastructure.Common.JWT;
using Infrastructure.Common.Mappers.Auth;
using Infrastructure.Data;
using Infrastructure.Data.UnitOfWork;
using Infrastructure.Repository.Interfaces;
using Infrastructure.Repository.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

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
            opts.UseSqlServer(builder.Configuration.GetConnectionString("DataContext"));
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

        services.AddControllers();

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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.JwtKey)),
                    ValidIssuer = jwtConfig.JwtIssuer,
                    ValidAudience = jwtConfig.JwtAudience,
                    ClockSkew = TimeSpan.Zero
                };
                o.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        var request = context.Request;
                        if (request != null && request.Cookies != null && request.Cookies.TryGetValue(AppConstants.AccessTokenCookie, out var accessToken) && !string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }

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
        services.AddScoped(typeof(Infrastructure.Repository.Interfaces.IGenericRepository<Domain.Models.Auth.AccessToken>), typeof(Infrastructure.Repository.Services.GenericRepository<Domain.Models.Auth.AccessToken>));
        services.AddScoped(typeof(Infrastructure.Repository.Interfaces.IGenericRepository<Domain.Models.Auth.RefreshToken>), typeof(Infrastructure.Repository.Services.GenericRepository<Domain.Models.Auth.RefreshToken>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
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
            // Security requirement removed to simplify Swagger config for now
        });
    }
}
