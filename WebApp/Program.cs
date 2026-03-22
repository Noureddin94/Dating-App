using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebApp.Application.Services;
using WebApp.Components;
using WebApp.Components.Services;
using WebApp.Domain.Interfaces;
using WebApp.Domain.Seeders;
using WebApp.Infrastructure.Domain.Interfaces;
using WebApp.Infrastructure.Infrastructure.Data;
using WebApp.Infrastructure.Repositories;

namespace WebApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Database ──────────────────────────────────────────────────────────
        builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

        // ── Identity ──────────────────────────────────────────────────────────
        builder.Services
            .AddIdentityApiEndpoints<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        // ── Auth ──────────────────────────────────────────────────────────────
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));
            options.AddPolicy("ApprovedUser", policy =>
                policy.RequireRole("ApprovedUser", "Admin"));
        });

        // ── Repository ────────────────────────────────────────────────────────
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // ── Application Services ──────────────────────────────────────────────
        builder.Services.AddScoped<IProfileService, ProfileService>();
        builder.Services.AddScoped<ILikeService, LikeService>();
        builder.Services.AddScoped<IMatchService, MatchService>();
        builder.Services.AddScoped<IGameService, GameService>();
        builder.Services.AddScoped<IMessageService, MessageService>();
        builder.Services.AddScoped<IModerationService, ModerationService>();
        builder.Services.AddScoped<IAdminService, AdminService>();

        // ── MVC + Razor Pages ─────────────────────────────────────────────────
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        // ── Blazor Server ─────────────────────────────────────────────────────
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
        
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
                Contact = new OpenApiContact { Name = "Dev", Email = "Nourkeswane2010@msn.com" }
            });
        });

        // ── CORS ──────────────────────────────────────────────────────────────
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        // ── Location Service ─────────────────────────────────────────────────────
        builder.Services.AddHttpClient<ILocationService, LocationService>(client =>
        {
            client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
            client.Timeout = TimeSpan.FromSeconds(10);
            // Nominatim requires a User-Agent header identifying your app — OSM policy
            client.DefaultRequestHeaders.Add("User-Agent", "LoveMatchApp/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        // ── Random User Service ─────────────────────────────────────────────────────
        builder.Services.AddHttpClient<IRandomUserService, RandomUserService>(client =>
        {
            client.BaseAddress = new Uri("https://randomuser.me/");
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        var app = builder.Build();
        Console.WriteLine($">>> Using connection: {app.Configuration.GetConnectionString("DefaultConnection")}");

        app.UseCors();
        app.UseSwagger();
        app.UseSwaggerUI(o =>
            o.SwaggerEndpoint("/swagger/v1/swagger.json", "Dating App API v1"));

        if (app.Environment.IsDevelopment())
            app.UseDeveloperExceptionPage();
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

        app.MapIdentityApi<IdentityUser>();
        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
        app.MapRazorPages();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        // uncomment when SignalR hubs are built:
        // app.MapHub<ChatHub>("/hubs/chat");
        // app.MapHub<GameHub>("/hubs/game");
        // app.MapHub<NotificationHub>("/hubs/notifications");
        // Auto-migrate and seed
        using (var scope = app.Services.CreateScope())
        {
            var logger = scope.ServiceProvider
                .GetRequiredService<ILogger<Program>>();
            try
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                logger.LogInformation("Applying migrations...");
                await db.Database.MigrateAsync();
                logger.LogInformation("Migrations applied.");

                logger.LogInformation("Seeding data...");
                await DataSeeder.SeedAsync(scope.ServiceProvider);
                logger.LogInformation("Seeding complete.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during migration or seeding.");
                throw; // re-throw so the app doesn't start with broken data
            }
        }

        app.Run();
    }
}
