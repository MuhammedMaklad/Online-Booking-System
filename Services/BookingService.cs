using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Contracts;
using Online_Booking_System.Data;
using Online_Booking_System.Models.Bookings;
using Online_Booking_System.ViewModels;

namespace Online_Booking_System.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        

        public async Task<bool> CreateBookingAsync(
            CreateBookingViewModel model,
            string userId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == model.PropertyId);

            if (property == null)
                return false;

            var nights = (model.CheckOut - model.CheckIn).Days;

            if (nights <= 0)
                return false;

            // Check availability: only Approved and Paid bookings block dates
            if (!await IsPropertyAvailableAsync(model.PropertyId, model.CheckIn, model.CheckOut))
                return false;

            var totalPrice = nights * property.PricePerNight;

            var booking = new Booking
            {
                PropertyId = model.PropertyId,
                UserId = userId,
                CheckIn = model.CheckIn,
                CheckOut = model.CheckOut,
                GuestsCount = model.GuestsCount,
                TotalPrice = totalPrice,
                Status = BookingStatus.Pending
            };

            _context.Bookings.Add(booking);

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AdminCancelBookingAsync(int bookingId, string adminId)
        {
            // adminId currently unused but kept for symmetry and auditing
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null) return false;

            // If booking is paid -> request refund, otherwise mark cancelled by admin
            if (booking.Status == BookingStatus.Paid)
            {
                booking.Status = BookingStatus.RefundPending;
            }
            else
            {
                booking.Status = BookingStatus.CancelledByAdmin;
            }

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MyBookingViewModel>> GetUserBookingsAsync(string userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Property)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt) 
                .Select(b => new MyBookingViewModel
                {
                    BookingId = b.Id,
                    PropertyTitle = b.Property.Title,
                    PropertyCity = b.Property.City,
                    PropertyImageUrl = b.Property.ImageUrl,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    GuestsCount = b.GuestsCount,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString()
                })
                .ToListAsync();

            return bookings;
        }
        public async Task<bool> CancelBookingAsync(int bookingId, string userId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null)
                return false;

            // Role: User cancelling their own booking
            // Allowed when Pending or Approved (before payment)
            if (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Approved)
            {
                booking.Status = BookingStatus.CancelledByUser;
            }
            else if (booking.Status == BookingStatus.Paid)
            {
                // User can request cancellation only before check-in
                if (DateTime.UtcNow < booking.CheckIn)
                {
                    booking.Status = BookingStatus.RefundPending;
                }
                else
                {
                    return false; // cannot cancel after check-in
                }
            }
            else
            {
                return false;
            }

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return true;
        }

        // Returns true if no Approved or Paid booking overlaps the requested interval
        public async Task<bool> IsPropertyAvailableAsync(int propertyId, DateTime checkIn, DateTime checkOut)
        {
            // Overlap definition: existing.CheckIn < requested.CheckOut && existing.CheckOut > requested.CheckIn
            return !await _context.Bookings
                .AnyAsync(b => b.PropertyId == propertyId
                               && (b.Status == BookingStatus.Approved || b.Status == BookingStatus.Paid)
                               && b.CheckIn < checkOut && b.CheckOut > checkIn);
        }
    }
}