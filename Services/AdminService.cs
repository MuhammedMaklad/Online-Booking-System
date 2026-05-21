using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Contracts;
using Online_Booking_System.Data;
using Online_Booking_System.Models.Advertisements;
using Online_Booking_System.Models.Bookings;
using Online_Booking_System.Models.Payments;
using Online_Booking_System.Models.Properties;
using Online_Booking_System.ViewModels;

namespace Online_Booking_System.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context) => _context = context;

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var totalProps = await _context.Properties.CountAsync();
            var pendingProps = await _context.Properties.CountAsync(p => p.Status == PropertyStatus.Pending);
            var approvedProps = await _context.Properties.CountAsync(p => p.Status == PropertyStatus.Approved);
            var totalBook = await _context.Bookings.CountAsync();
            var pendingBook = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
            // Revenue should be based on completed payment transactions
            var totalRevenue = await _context.PaymentTransactions
                .Where(t => t.Status == PaymentStatus.Completed)
                .SumAsync(t => t.Amount);
            var totalAds = await _context.Advertisements.CountAsync();
            var pendingAds = await _context.Advertisements.CountAsync(a => a.Status == AdvertisementStatus.Pending);
            var completedPay = await _context.PaymentTransactions.CountAsync(t => t.Status == PaymentStatus.Completed);
            var payRevenue = await _context.PaymentTransactions
                .Where(t => t.Status == PaymentStatus.Completed)
                .SumAsync(t => t.Amount);

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt).Take(3)
                .Select(u => new AdminRecentActivityViewModel
                {
                    Type = "User",
                    Description = $"New user: {u.FirstName} {u.LastName}",
                    CreatedAt = u.CreatedAt
                }).ToListAsync();

            var recentProps = await _context.Properties
                .OrderByDescending(p => p.CreatedAt).Take(3)
                .Select(p => new AdminRecentActivityViewModel
                {
                    Type = "Property",
                    Description = $"New listing: {p.Title}",
                    CreatedAt = p.CreatedAt
                }).ToListAsync();

            return new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalProperties = totalProps,
                PendingProperties = pendingProps,
                ApprovedProperties = approvedProps,
                TotalBookings = totalBook,
                PendingBookings = pendingBook,
                TotalRevenue = totalRevenue,
                TotalAdvertisements = totalAds,
                PendingAdvertisements = pendingAds,
                CompletedPayments = completedPay,
                TotalPaymentRevenue = payRevenue,
                RecentActivity = recentUsers.Concat(recentProps)
                    .OrderByDescending(a => a.CreatedAt).Take(8).ToList()
            };
        }

        public async Task<AdminPropertyFilterViewModel> GetPropertiesAsync(AdminPropertyFilterViewModel filter)
        {
            var query = _context.Properties.Include(p => p.Owner).AsQueryable();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var s = filter.Search.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(s) ||
                    p.City.ToLower().Contains(s) || p.Country.ToLower().Contains(s));
            }

            if (!string.IsNullOrEmpty(filter.Status) &&
                Enum.TryParse<PropertyStatus>(filter.Status, out var st))
                query = query.Where(p => p.Status == st);

            filter.TotalCount = await query.CountAsync();
            filter.Properties = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
                .Select(p => new AdminPropertyListViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    City = p.City,
                    Country = p.Country,
                    PricePerNight = p.PricePerNight,
                    ImageUrl = p.ImageUrl,
                    OwnerName = p.Owner != null ? p.Owner.FirstName + " " + p.Owner.LastName : "Unknown",
                    OwnerEmail = p.Owner != null ? p.Owner.Email ?? "" : "",
                    Status = p.Status.ToString(),
                    AdminNotes = p.AdminNotes,
                    CreatedAt = p.CreatedAt
                }).ToListAsync();
            return filter;
        }

        public async Task<AdminPropertyListViewModel?> GetPropertyByIdAsync(int id) =>
            await _context.Properties.Include(p => p.Owner).Where(p => p.Id == id)
                .Select(p => new AdminPropertyListViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    City = p.City,
                    Country = p.Country,
                    PricePerNight = p.PricePerNight,
                    ImageUrl = p.ImageUrl,
                    OwnerName = p.Owner != null ? p.Owner.FirstName + " " + p.Owner.LastName : "Unknown",
                    OwnerEmail = p.Owner != null ? p.Owner.Email ?? "" : "",
                    Status = p.Status.ToString(),
                    AdminNotes = p.AdminNotes,
                    CreatedAt = p.CreatedAt
                }).FirstOrDefaultAsync();

        public async Task<bool> ApprovePropertyAsync(int id, string? notes)
        {
            var p = await _context.Properties.FindAsync(id);
            if (p == null) return false;
            p.Status = PropertyStatus.Approved; p.AdminNotes = notes;
            await _context.SaveChangesAsync(); return true;
        }

        public async Task<bool> RejectPropertyAsync(int id, string? notes)
        {
            var p = await _context.Properties.FindAsync(id);
            if (p == null) return false;
            p.Status = PropertyStatus.Rejected; p.AdminNotes = notes;
            await _context.SaveChangesAsync(); return true;
        }

        public async Task<bool> DeletePropertyAsync(int id)
        {
            var p = await _context.Properties.FindAsync(id);
            if (p == null) return false;
            _context.Properties.Remove(p); await _context.SaveChangesAsync(); return true;
        }

        public async Task<AdminAdvertisementFilterViewModel> GetAdvertisementsAsync(AdminAdvertisementFilterViewModel filter)
        {
            var query = _context.Advertisements.Include(a => a.Owner).AsQueryable();

            if (!string.IsNullOrEmpty(filter.Status) &&
                Enum.TryParse<AdvertisementStatus>(filter.Status, out var st))
                query = query.Where(a => a.Status == st);

            filter.TotalCount = await query.CountAsync();
            filter.Advertisements = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
                .Select(a => new AdminAdvertisementListViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    ImageUrl = a.ImageUrl,
                    OwnerName = a.Owner != null ? a.Owner.FirstName + " " + a.Owner.LastName : "Unknown",
                    OwnerEmail = a.Owner != null ? a.Owner.Email ?? "" : "",
                    Status = a.Status.ToString(),
                    AdminNotes = a.AdminNotes,
                    CreatedAt = a.CreatedAt
                }).ToListAsync();
            return filter;
        }

        public async Task<bool> ApproveAdvertisementAsync(int id, string? notes)
        {
            var a = await _context.Advertisements.FindAsync(id);
            if (a == null) return false;
            a.Status = AdvertisementStatus.Approved; a.AdminNotes = notes; a.ReviewedAt = DateTime.Now;
            await _context.SaveChangesAsync(); return true;
        }

        public async Task<bool> RejectAdvertisementAsync(int id, string? notes)
        {
            var a = await _context.Advertisements.FindAsync(id);
            if (a == null) return false;
            a.Status = AdvertisementStatus.Rejected; a.AdminNotes = notes; a.ReviewedAt = DateTime.Now;
            await _context.SaveChangesAsync(); return true;
        }

        public async Task<bool> DeleteAdvertisementAsync(int id)
        {
            var a = await _context.Advertisements.FindAsync(id);
            if (a == null) return false;
            _context.Advertisements.Remove(a); await _context.SaveChangesAsync(); return true;
        }

        //public async Task<IEnumerable<BookingsViewModel>> GetAllBookingsAsync()
        //{
        //    var bookings = await _context.Bookings
        //        .Include(b => b.Property)
        //        .Include(b => b.User)
        //        .OrderByDescending(b => b.CreatedAt)
        //        .Select(b => new BookingsViewModel
        //        {
        //            BookingId = b.Id,
        //            PropertyTitle = b.Property.Title,
        //            PropertyCity = b.Property.City,
        //            PropertyImageUrl = b.Property.ImageUrl,
        //            UserName = b.User.UserName,
        //            CheckIn = b.CheckIn,
        //            CheckOut = b.CheckOut,
        //            GuestsCount = b.GuestsCount,
        //            TotalPrice = b.TotalPrice,
        //            Status = b.Status.ToString()
        //        })
        //        .ToListAsync();

        //    return bookings;
        //}
    }
}