using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Models;

namespace Online_Booking_System.Data
{
  public class AppDbContext:IdentityDbContext<ApplicationUser, ApplicationRole, string>
  {
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
    {
      
    }

    //public override
  }
}
