using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Contracts;
using Online_Booking_System.Data;
using Online_Booking_System.Models;
using Online_Booking_System.Services;
using Online_Booking_System.Settings;

namespace Online_Booking_System
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);


      builder.Services.AddDbContext<AppDbContext>(options =>
          options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

      builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
          options.Password.RequireDigit = true;
          options.Password.RequireLowercase = true;
          options.Password.RequireUppercase = true;
          options.Password.RequireNonAlphanumeric = false;
          options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

            builder.Services.AddAuthentication()
              .AddGoogle(options =>
              {
                  options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
                  options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
                  options.CallbackPath = "/signin-google";
              });

            builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
      builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

      builder.Services.AddScoped<IEmailService, EmailService>();
      builder.Services.AddScoped<IUserService, UserService>();
      builder.Services.AddScoped<IPropertyService, PropertyService>();
            builder.Services.AddScoped<IBookingService, BookingService>();

            builder.Services.AddControllersWithViews();

      var app = builder.Build();

      if (!app.Environment.IsDevelopment())
      {
        app.UseExceptionHandler("/Home/Error");
      }

      app.UseStaticFiles();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.MapControllerRoute(
          name: "default",
          pattern: "{controller=Home}/{action=Index}/{id?}")
          .WithStaticAssets();

      using (var scope = app.Services.CreateScope())
      {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
      }

      app.Run();
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
      var roles = new[] { "Admin", "User", "Guest" };

      foreach (var role in roles)
      {
        if (!await roleManager.RoleExistsAsync(role))
        {
          await roleManager.CreateAsync(new ApplicationRole { Name = role });
        }
      }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
      var adminEmail = "admin@booking.com";
      var adminPassword = "Admin123@";

      if (await userManager.FindByEmailAsync(adminEmail) is null)
      {
        var admin = new ApplicationUser
        {
          FirstName = "System",
          LastName = "Admin",
          Email = adminEmail,
          UserName = adminEmail,
          IsActive = true,
          CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, adminPassword);

        if (result.Succeeded)
        {
          await userManager.AddToRoleAsync(admin, "Admin");
        }
      }
    }
  }
}