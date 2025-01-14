using Application.Interfaces;
using CryptoOrganizerWebAPI.Interfaces;
using CryptoOrganizerWebAPI.Services;
using Domain.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

namespace CryptoOrganizerWebAPI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("./Logs/logfile.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddDbContext<CryptoOrganizerContext>(options => options.UseInMemoryDatabase("CryptoOrganizerDatabase"));

            builder.Services.AddIdentity<CryptoOrganizerUser, IdentityRole>()
                            .AddEntityFrameworkStores<CryptoOrganizerContext>()
                            .AddDefaultTokenProviders();

            builder.Services.AddMemoryCache();

            builder.Services.AddScoped<ICryptoOrganizerContext>(provider => provider.GetRequiredService<CryptoOrganizerContext>());
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ICgService, CgService>();
            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            builder.Services.AddSingleton<ICacheService, CacheService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost4200", builder =>
                {
                    builder.WithOrigins("http://localhost:4200")
                           .AllowAnyHeader()
                           .AllowCredentials()
                           .AllowAnyMethod();
                });
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
            });


            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowLocalhost4200");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}