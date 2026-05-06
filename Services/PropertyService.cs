using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Contracts;
using Online_Booking_System.Data;
using Online_Booking_System.Models.Properties;
using Online_Booking_System.ViewModels;
namespace Online_Booking_System.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly AppDbContext _context;

        public PropertyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Property>> GetFilteredAsync(PropertyFilterViewModel filter)
        {
            var query = _context.Properties.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(p =>
                    p.Title.Contains(filter.SearchTerm) ||
                    p.Description.Contains(filter.SearchTerm));
            }

            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                query = query.Where(p => p.City == filter.City);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.PricePerNight >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.PricePerNight <= filter.MaxPrice.Value);
            }
            if (filter.Bedrooms.HasValue)
            {
                query = query.Where(p =>
                    p.Bedrooms >= filter.Bedrooms.Value);
            }
            query = filter.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.PricePerNight),

                "price_desc" => query.OrderByDescending(p => p.PricePerNight),

                _ => query.OrderByDescending(p => p.CreatedAt)
            };
            var totalCount = await query.CountAsync();

            filter.TotalPages = (int)Math.Ceiling(
                totalCount / (double)filter.PageSize);

            return await query
                            .Skip((filter.PageNumber - 1) * filter.PageSize)
                            .Take(filter.PageSize)
                             .ToListAsync();
        }
        public async Task<Property?> GetByIdAsync(int id)
        {
            return await _context.Properties
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}