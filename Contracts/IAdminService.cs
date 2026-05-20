using Online_Booking_System.ViewModels;

namespace Online_Booking_System.Contracts
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();

        Task<AdminPropertyFilterViewModel> GetPropertiesAsync(AdminPropertyFilterViewModel filter);
        Task<AdminPropertyListViewModel?> GetPropertyByIdAsync(int propertyId);
        Task<bool> ApprovePropertyAsync(int propertyId, string? adminNotes);
        Task<bool> RejectPropertyAsync(int propertyId, string? adminNotes);
        Task<bool> DeletePropertyAsync(int propertyId);

        Task<AdminAdvertisementFilterViewModel> GetAdvertisementsAsync(AdminAdvertisementFilterViewModel filter);
        Task<bool> ApproveAdvertisementAsync(int adId, string? adminNotes);
        Task<bool> RejectAdvertisementAsync(int adId, string? adminNotes);
        Task<bool> DeleteAdvertisementAsync(int adId);
    }
}