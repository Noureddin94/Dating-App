using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebApp.Infrastructure.Infrastructure.Data;
using WebApp.Components;

namespace WebApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Database ──────────────────────────────────────────────────────────
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo")
                ));

        // ── Identity ──────────────────────────────────────────────────────────
        builder.Services
            .AddIdentityApiEndpoints<IdentityUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        // ── Auth ──────────────────────────────────────────────────────────────
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        // ── MVC + Razor Pages ─────────────────────────────────────────────────
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        // ── Blazor Server ─────────────────────────────────────────────────────
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // ── SignalR ───────────────────────────────────────────────────────────
        builder.Services.AddSignalR();

        // ── Swagger ───────────────────────────────────────────────────────────
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme,
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "JWT Authorization header using Bearer scheme",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        BearerFormat = "JWT",
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Dating App API",
                Description = "ASP.NET Core 9 Web API",
                Contact = new OpenApiContact { Name = "Dev", Email = "dev@example.nl" }
            });
        });

        // ── CORS ──────────────────────────────────────────────────────────────
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        // ─────────────────────────────────────────────────────────────────────
        var app = builder.Build();
        // ─────────────────────────────────────────────────────────────────────

        app.UseCors();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(o =>
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "Dating App API v1"));
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        // Identity minimal API (/register, /login, /logout, etc.)
        app.MapIdentityApi<IdentityUser>();

        // MVC controllers
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        // Razor Pages (Identity scaffolded UI under /Identity/Account/...)
        app.MapRazorPages();

        // Blazor Server — maps /_blazor hub and serves App.razor as the root
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // SignalR hubs — uncomment as you add them
        // app.MapHub<ChatHub>("/hubs/chat");
        // app.MapHub<GameHub>("/hubs/game");

        app.Run();
    }
}
