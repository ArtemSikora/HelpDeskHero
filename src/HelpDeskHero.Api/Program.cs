using System.Text;

using HelpDeskHero.Api.Domain;
using HelpDeskHero.Api.Infrastructure;
using HelpDeskHero.Api.Infrastructure.Persistence;
using HelpDeskHero.Api.Security;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder =
    WebApplication
        .CreateBuilder(
            args);

const string CorsPolicyName =
    "BlazorUi";

builder.Services
    .AddCors(
        options =>
        {
            options.AddPolicy(
                CorsPolicyName,
                policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5145",
                            "https://localhost:5145")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

builder.Services
    .AddDbContext<AppDbContext>(
        options =>
            options.UseSqlite(
                builder.Configuration
                    .GetConnectionString(
                        "DefaultConnection")));

builder.Services
    .AddIdentity<
        ApplicationUser,
        IdentityRole>(
        options =>
        {
            options.Password
                .RequireDigit =
                true;

            options.Password
                .RequireUppercase =
                true;

            options.Password
                .RequireLowercase =
                true;

            options.Password
                .RequireNonAlphanumeric =
                false;

            options.Password
                .RequiredLength =
                8;

            options.User
                .RequireUniqueEmail =
                false;
        })
    .AddEntityFrameworkStores<
        AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .Configure<JwtOptions>(
        builder.Configuration
            .GetSection(
                "Jwt"));

builder.Services
    .AddScoped<
        ITokenService,
        TokenService>();

builder.Services
    .AddScoped<
        RefreshTokenService>();

builder.Services
    .AddScoped<
        AuditService>();

var jwtSection =
    builder.Configuration
        .GetSection(
            "Jwt");

var jwtKey =
    jwtSection["Key"]
    ?? throw new InvalidOperationException(
        "Missing Jwt:Key");

var signingKey =
    new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(
            jwtKey));

builder.Services
    .AddAuthentication(
        options =>
        {
            options
                .DefaultAuthenticateScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;

            options
                .DefaultChallengeScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;

            options
                .DefaultScheme =
                JwtBearerDefaults
                    .AuthenticationScheme;
        })
    .AddJwtBearer(
        options =>
        {
            options
                .TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer =
                        true,

                    ValidateAudience =
                        true,

                    ValidateIssuerSigningKey =
                        true,

                    ValidateLifetime =
                        true,

                    ValidIssuer =
                        jwtSection[
                            "Issuer"],

                    ValidAudience =
                        jwtSection[
                            "Audience"],

                    IssuerSigningKey =
                        signingKey,

                    ClockSkew =
                        TimeSpan.Zero
                };
        });

builder.Services
    .AddAuthorization(
        options =>
        {
            options
                .AddPolicy(
                    "AdminOnly",
                    policy =>
                        policy
                            .RequireRole(
                                "Admin"));

            options
                .AddPolicy(
                    "AgentOrAdmin",
                    policy =>
                        policy
                            .RequireRole(
                                "Agent",
                                "Admin"));

            options
                .AddPolicy(
                    "CanManageTickets",
                    policy =>
                        policy
                            .RequireRole(
                                "User",
                                "Agent",
                                "Admin"));
        });

builder.Services
    .AddControllers();

builder.Services
    .AddEndpointsApiExplorer();

builder.Services
    .AddSwaggerGen(
        options =>
        {
            options
                .AddSecurityDefinition(
                    "Bearer",

                    new OpenApiSecurityScheme
                    {
                        Name =
                            "Authorization",

                        Type =
                            SecuritySchemeType
                                .Http,

                        Scheme =
                            "bearer",

                        BearerFormat =
                            "JWT",

                        In =
                            ParameterLocation
                                .Header
                    });

            options
                .AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference =
                                    new OpenApiReference
                                    {
                                        Type =
                                            ReferenceType
                                                .SecurityScheme,

                                        Id =
                                            "Bearer"
                                    }
                            },

                            Array.Empty<string>()
                        }
                    });
        });

var app =
    builder
        .Build();

await DbSeeder
    .SeedAsync(
        app.Services);

if (app.Environment
        .IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseCors(
    CorsPolicyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
