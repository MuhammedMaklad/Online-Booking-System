using Online_Booking_System.ViewModels;

namespace Online_Booking_System.Contracts
{
  public interface IUserService
  {
    Task<(string? UserId, string? Token)> RegisterAsync(RegisterViewModel model);
    Task<bool> ConfirmEmailAsync(ConfirmEmailViewModel model);
    Task<string?> ResendConfirmationEmailAsync(string email);
    Task<LoginViewModel> LoginAsync(LoginViewModel model);
    Task LogoutAsync();
    Task<bool> SendPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordViewModel model);
    Task<UserProfileViewModel?> GetWithProfileAsync(string userId);
    Task<bool> UpdateProfileAsync(string userId, EditProfileViewModel model);
    Task<(bool Success, string? Error)> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
    Task<UserListViewModel> GetUsersAsync(UserFilterViewModel filter);
    Task<bool> SetActiveStatusAsync(string userId, bool isActive);
    Task<bool> AssignRoleAsync(string userId, string role);
    Task<bool> RevokeRoleAsync(string userId, string role);
    Task<bool> RemoveUserAsync(string userId);
    Task<List<string>> GetUserRolesAsync(string userId);
  }
}