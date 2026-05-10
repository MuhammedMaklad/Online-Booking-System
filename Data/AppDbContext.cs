using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Models;
using Online_Booking_System.Models.Properties;
using Online_Booking_System.Models.Bookings;
using Online_Booking_System.Models.Payments;

namespace Online_Booking_System.Data
{
  public class AppDbContext:IdentityDbContext<ApplicationUser, ApplicationRole, string>
  {
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
    {
      
    }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Prevent multiple cascade paths on PaymentTransactions
            builder.Entity<PaymentTransaction>()
                .HasOne(t => t.Booking)
                .WithMany()
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PaymentTransaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
