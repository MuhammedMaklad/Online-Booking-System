using Online_Booking_System.Models.Properties;
using Online_Booking_System.ViewModels;

namespace Online_Booking_System.Contracts
{
    public interface IPropertyService
    {
        Task<IEnumerable<Property>> GetFilteredAsync(PropertyFilterViewModel filter);
        Task<Property?> GetByIdAsync(int id);
    }
}