using CryptoProject.Components;
using CryptoProject.Components.Account;
using CryptoProject.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Radzen;

namespace CryptoProject
{
    public class Program
    {
#if DEBUG
        public const string CONNECTION_STRING = "Host=localhost; Database=CryptoProject; Username=postgres; Password=root;";
#else
        public const string CONNECTION_STRING = "Host=postgres; Database=CryptoProject; Username=postgres; Password=root;";
#endif
        public static async Task AssignAdminRole(IServiceProvider serviceProvider, string email)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                Console.WriteLine($"User with email {email} not found.");
                return;
            }

            if (await userManager.IsInRoleAsync(user, "Admin"))
            {
                Console.WriteLine($"User {email} is already in the Admin role.");
                return;
            }

            var result = await userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                await userManager.UpdateSecurityStampAsync(user);
                Console.WriteLine($"User {email} has been assigned the Admin role.");
            }
            else
            {
                Console.WriteLine($"Failed to assign Admin role to {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        public static async Task SeedRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Add required roles
            var roles = new[] { "User", "Moderator", "Admin" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddScoped<DialogService>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdminRole", policy =>
                    policy.RequireRole("Admin"));
                options.AddPolicy("RequireAdminOrModeratorRole", policy =>
                    policy.RequireRole("Admin", "Moderator"));
            });

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseNpgsql(CONNECTION_STRING));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            builder.Services.AddRadzenComponents();

            var app = builder.Build();

            // ¬ыполнение миграций перед запуском приложени€
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    // Apply database migrations
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    logger.LogInformation("Applying database migrations...");
                    context.Database.Migrate();
                    logger.LogInformation("Database migrations applied successfully.");

                    // Seed roles
                    await SeedRoles(services);
                    await AssignAdminRole(services, "weizedinc@gmail.com");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating the database or seeding roles.");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
