using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Models;
using Online_Booking_System.Models.Properties;
using Online_Booking_System.Models.Bookings;

namespace Online_Booking_System.Data
{
  public class AppDbContext:IdentityDbContext<ApplicationUser, ApplicationRole, string>
  {
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
    {
      
    }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        //public override
    }
}
