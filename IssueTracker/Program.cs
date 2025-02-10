using IssueTracker.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace IssueTracker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<IssuesDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
            }
            )
            .AddEntityFrameworkStores<IssuesDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.Redirect("/Home/Unauthorized");
                        return Task.CompletedTask;
                    },
                    OnRedirectToLogin = context =>
                    {
                        context.Response.Redirect("/Account/Login");
                        return Task.CompletedTask;
                    }
                };
            });
            

            var app = builder.Build();

            //Seed Roles
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roles = new[] { "Admin", "User", "Developer", "Tester" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
                string adminEmail = "admin@example.com";
                string adminPassword = "Admin@123";

                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                    await userManager.CreateAsync(adminUser, adminPassword);
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                }
                // Developer user seeding
                string developerEmail = "developer@example.com";
                string developerPassword = "Dev@123";
                var developerUser = await userManager.FindByEmailAsync(developerEmail);

                if (developerUser == null)
                {
                    developerUser = new IdentityUser { UserName = developerEmail, Email = developerEmail };
                    await userManager.CreateAsync(developerUser, developerPassword);
                    await userManager.AddToRoleAsync(developerUser, "Developer");
                }

                // Tester user seeding
                string testerEmail = "tester@example.com";
                string testerPassword = "Test@123";
                var testerUser = await userManager.FindByEmailAsync(testerEmail);

                if (testerUser == null)
                {
                    testerUser = new IdentityUser { UserName = testerEmail, Email = testerEmail };
                    await userManager.CreateAsync(testerUser, testerPassword);
                    await userManager.AddToRoleAsync(testerUser, "Tester");
                }

            }



            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}
