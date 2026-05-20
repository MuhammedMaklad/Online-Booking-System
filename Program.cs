using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Contracts;
using Online_Booking_System.Contracts.Payment;
using Online_Booking_System.Data;
using Online_Booking_System.Models;
using Online_Booking_System.Services;
using Online_Booking_System.Services.Payment;
using Online_Booking_System.Settings;

namespace Online_Booking_System
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);

      if (builder.Environment.IsDevelopment())
      {
        builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
      }

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
            builder.Services.AddScoped<IOwnerService, OwnerService>();
            builder.Services.AddScoped<IAdminService, AdminService>();


            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
            builder.Services.Configure<PayMobSettings>(builder.Configuration.GetSection("PayMob"));
            builder.Services.Configure<PayPalSettings>(builder.Configuration.GetSection("PayPal"));

            builder.Services.AddScoped<IPaymentProvider, StripePaymentProvider>();
            builder.Services.AddScoped<IPaymentProvider, PayMobPaymentProvider>();
            builder.Services.AddScoped<IPaymentProvider, PayPalPaymentProvider>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();

            builder.Services.AddHttpClient("PayMob");
            builder.Services.AddHttpClient("PayPal");

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllersWithViews();

      var app = builder.Build();

      if (!app.Environment.IsDevelopment())
      {
           app.UseExceptionHandler("/Home/Error");
           app.UseStatusCodePagesWithReExecute("/Home/ErrorPage/{0}");
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
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedOwnerUserAsync(userManager, dbContext);
      }

      app.Run();
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
      var roles = new[] { "Admin", "User", "Owner" };

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

    private static async Task SeedOwnerUserAsync(
      UserManager<ApplicationUser> userManager,
      AppDbContext dbContext)
    {
      var ownerEmail = "owner@booking.com";
      var ownerPassword = "Owner123@";

      var owner = await userManager.FindByEmailAsync(ownerEmail);

      if (owner is null)
      {
        owner = new ApplicationUser
        {
          FirstName = "John",
          LastName = "Owner",
          Email = ownerEmail,
          UserName = ownerEmail,
          EmailConfirmed = true,
          IsActive = true,
          Bio = "Experienced property host with listings across multiple cities.",
          CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(owner, ownerPassword);

        if (!result.Succeeded)
          return;

        await userManager.AddToRoleAsync(owner, "Owner");
      }

      if (dbContext.Properties.Any(p => p.OwnerId == owner.Id))
        return;

      var properties = new[]
      {
        new Online_Booking_System.Models.Properties.Property
        {
          Title = "Luxury Beachfront Villa",
          Description = "A stunning beachfront villa with panoramic ocean views, private pool, and direct beach access. Perfect for families and groups seeking a premium coastal retreat.",
          Facilities = "WiFi, Pool, Air Conditioning, Parking, Kitchen, Sea View, Balcony",
          PricePerNight = 450,
          Address = "12 Ocean Drive",
          City = "Miami",
          Country = "USA",
          ImageUrl = "https://images.unsplash.com/photo-1613490493576-7fde63acd811?w=800",
          GalleryImages = "https://images.unsplash.com/photo-1613490493576-7fde63acd811?w=800,https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=800",
          GoogleMapUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3592.5!2d-80.13!3d25.77!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x0%3A0x0!2zMjXCsDQ2JzEyLjAiTiA4MMKwMDcnNDguMCJX!5e0!3m2!1sen!2sus!4v1620000000000!5m2!1sen!2sus",
          Bedrooms = 4,
          Bathrooms = 3,
          MaxGuests = 8,
          Rating = 4.9,
          ReviewsCount = 124,
          Status = Online_Booking_System.Models.Properties.PropertyStatus.Approved,
          OwnerId = owner.Id,
          CreatedAt = DateTime.Now.AddDays(-60)
        },
        new Online_Booking_System.Models.Properties.Property
        {
          Title = "Modern Downtown Apartment",
          Description = "Sleek and stylish apartment in the heart of the city. Walking distance to top restaurants, museums, and entertainment. Ideal for business travelers and city explorers.",
          Facilities = "WiFi, Air Conditioning, Kitchen, Netflix, Gym",
          PricePerNight = 120,
          Address = "88 Central Avenue, Floor 15",
          City = "New York",
          Country = "USA",
          ImageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800",
          GalleryImages = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800,https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?w=800",
          Bedrooms = 1,
          Bathrooms = 1,
          MaxGuests = 2,
          Rating = 4.7,
          ReviewsCount = 89,
          Status = Online_Booking_System.Models.Properties.PropertyStatus.Approved,
          OwnerId = owner.Id,
          CreatedAt = DateTime.Now.AddDays(-45)
        },
        new Online_Booking_System.Models.Properties.Property
        {
          Title = "Cozy Mountain Cabin",
          Description = "Escape to this charming log cabin nestled in the mountains. Features a fireplace, hot tub, and breathtaking forest views. Perfect for a romantic getaway or nature retreat.",
          Facilities = "WiFi, Parking, Kitchen, Balcony, Breakfast",
          PricePerNight = 185,
          Address = "7 Pine Ridge Road",
          City = "Aspen",
          Country = "USA",
          ImageUrl = "https://images.unsplash.com/photo-1449158743715-0a90ebb6d2d8?w=800",
          GalleryImages = "https://images.unsplash.com/photo-1449158743715-0a90ebb6d2d8?w=800,https://images.unsplash.com/photo-1510798831971-661eb04b3739?w=800",
          Bedrooms = 2,
          Bathrooms = 1,
          MaxGuests = 4,
          Rating = 4.8,
          ReviewsCount = 56,
          Status = Online_Booking_System.Models.Properties.PropertyStatus.Approved,
          OwnerId = owner.Id,
          CreatedAt = DateTime.Now.AddDays(-30)
        }
      };

      dbContext.Properties.AddRange(properties);
      await dbContext.SaveChangesAsync();
    }
  }
}
