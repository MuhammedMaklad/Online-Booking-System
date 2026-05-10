using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Contracts;
using Online_Booking_System.Data;
using Online_Booking_System.Models.Bookings;
using Online_Booking_System.Models.Properties;
using Online_Booking_System.ViewModels;

namespace Online_Booking_System.Services
{
    public class OwnerService : IOwnerService
    {
        private readonly AppDbContext _context;

        public OwnerService(AppDbContext context)
        {
            _context = context;
        }

        // ─── Dashboard ────────────────────────────────────────────────────────────

        public async Task<OwnerDashboardViewModel> GetDashboardAsync(string ownerId)
        {
            var propertyIds = await _context.Properties
                .Where(p => p.OwnerId == ownerId)
                .Select(p => p.Id)
                .ToListAsync();

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                .Where(b => propertyIds.Contains(b.PropertyId))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var recentProperties = await _context.Properties
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new OwnerPropertySummaryViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    City = p.City,
                    Country = p.Country,
                    PricePerNight = p.PricePerNight,
                    ImageUrl = p.ImageUrl,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    MaxGuests = p.MaxGuests,
                    TotalBookings = _context.Bookings.Count(b => b.PropertyId == p.Id),
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var recentBookings = bookings
                .Take(5)
                .Select(b => new OwnerBookingSummaryViewModel
                {
                    BookingId = b.Id,
                    PropertyTitle = b.Property.Title,
                    GuestName = b.User != null ? b.User.FullName : "Guest",
                    GuestEmail = b.User?.Email ?? string.Empty,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    GuestsCount = b.GuestsCount,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt
                })
                .ToList();

            return new OwnerDashboardViewModel
            {
                TotalProperties = propertyIds.Count,
                TotalBookings = bookings.Count,
                PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending),
                ConfirmedBookings = bookings.Count(b => b.Status == BookingStatus.Confirmed),
                CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                TotalRevenue = bookings
                    .Where(b => b.Status != BookingStatus.Cancelled)
                    .Sum(b => b.TotalPrice),
                RecentProperties = recentProperties,
                RecentBookings = recentBookings
            };
        }

        // ─── My Properties ────────────────────────────────────────────────────────

        public async Task<IEnumerable<OwnerPropertySummaryViewModel>> GetMyPropertiesAsync(string ownerId)
        {
            return await _context.Properties
                .Where(p => p.OwnerId == ownerId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new OwnerPropertySummaryViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    City = p.City,
                    Country = p.Country,
                    PricePerNight = p.PricePerNight,
                    ImageUrl = p.ImageUrl,
                    Bedrooms = p.Bedrooms,
                    Bathrooms = p.Bathrooms,
                    MaxGuests = p.MaxGuests,
                    TotalBookings = _context.Bookings.Count(b => b.PropertyId == p.Id),
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }

        // ─── Create Property ──────────────────────────────────────────────────────

        public async Task<int> CreatePropertyAsync(CreatePropertyViewModel model, string ownerId)
        {
            var property = new Property
            {
                Title = model.Title,
                Description = model.Description,
                Facilities = model.Facilities,
                PricePerNight = model.PricePerNight,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                ImageUrl = model.ImageUrl,
                GalleryImages = model.GalleryImages,
                GoogleMapUrl = model.GoogleMapUrl,
                Bedrooms = model.Bedrooms,
                Bathrooms = model.Bathrooms,
                MaxGuests = model.MaxGuests,
                OwnerId = ownerId,
                CreatedAt = DateTime.Now
            };

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            return property.Id;
        }

        // ─── Get Property For Edit ────────────────────────────────────────────────

        public async Task<EditPropertyViewModel?> GetPropertyForEditAsync(int propertyId, string ownerId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == propertyId && p.OwnerId == ownerId);

            if (property == null)
                return null;

            return new EditPropertyViewModel
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                Facilities = property.Facilities,
                PricePerNight = property.PricePerNight,
                Address = property.Address,
                City = property.City,
                Country = property.Country,
                ImageUrl = property.ImageUrl,
                GalleryImages = property.GalleryImages,
                GoogleMapUrl = property.GoogleMapUrl,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                MaxGuests = property.MaxGuests
            };
        }

        // ─── Update Property ──────────────────────────────────────────────────────

        public async Task<bool> UpdatePropertyAsync(EditPropertyViewModel model, string ownerId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == model.Id && p.OwnerId == ownerId);

            if (property == null)
                return false;

            property.Title = model.Title;
            property.Description = model.Description;
            property.Facilities = model.Facilities;
            property.PricePerNight = model.PricePerNight;
            property.Address = model.Address;
            property.City = model.City;
            property.Country = model.Country;
            property.ImageUrl = model.ImageUrl;
            property.GalleryImages = model.GalleryImages;
            property.GoogleMapUrl = model.GoogleMapUrl;
            property.Bedrooms = model.Bedrooms;
            property.Bathrooms = model.Bathrooms;
            property.MaxGuests = model.MaxGuests;

            _context.Properties.Update(property);
            await _context.SaveChangesAsync();

            return true;
        }

        // ─── Delete Property ──────────────────────────────────────────────────────

        public async Task<bool> DeletePropertyAsync(int propertyId, string ownerId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == propertyId && p.OwnerId == ownerId);

            if (property == null)
                return false;

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return true;
        }

        // ─── Property Bookings ────────────────────────────────────────────────────

        public async Task<PropertyBookingsViewModel?> GetPropertyBookingsAsync(int propertyId, string ownerId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == propertyId && p.OwnerId == ownerId);

            if (property == null)
                return null;

            var bookings = await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.PropertyId == propertyId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new OwnerBookingSummaryViewModel
                {
                    BookingId = b.Id,
                    PropertyTitle = property.Title,
                    GuestName = b.User != null ? b.User.FullName : "Guest",
                    GuestEmail = b.User != null ? b.User.Email! : string.Empty,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    GuestsCount = b.GuestsCount,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status.ToString(),
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return new PropertyBookingsViewModel
            {
                PropertyId = propertyId,
                PropertyTitle = property.Title,
                Bookings = bookings
            };
        }

        // ─── Confirm Booking ──────────────────────────────────────────────────────

        public async Task<bool> ConfirmBookingAsync(int bookingId, string ownerId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.Property.OwnerId == ownerId);

            if (booking == null || booking.Status != BookingStatus.Pending)
                return false;

            booking.Status = BookingStatus.Confirmed;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return true;
        }

        // ─── Cancel Booking ───────────────────────────────────────────────────────

        public async Task<bool> CancelBookingAsync(int bookingId, string ownerId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.Property.OwnerId == ownerId);

            if (booking == null || booking.Status == BookingStatus.Cancelled)
                return false;

            booking.Status = BookingStatus.Cancelled;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
