using System.ComponentModel.DataAnnotations;

namespace Online_Booking_System.ViewModels
{
  public class RegisterViewModel
  {
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;


        [Required]
        [MinLength(6)]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{6,}$",
    ErrorMessage = "Password must be at least 6 characters and contain one uppercase letter, one lowercase letter, and one digit.")]
        public string Password { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
  }

  public class ConfirmEmailViewModel
  {
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
  }

  public class ResendConfirmationViewModel
  {
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
  }

  public class LoginViewModel
  {
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? UserId { get; set; }
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
  }

  public class SendPasswordResetViewModel
  {
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
  }

  public class ResetPasswordViewModel
  {
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
  }

  public class UserProfileViewModel
  {
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Roles { get; set; } = [];
  }

  public class EditProfileViewModel
  {
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(300)]
    public string? Bio { get; set; }
  }

  public class ChangePasswordViewModel
  {
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
  }

  public class UserFilterViewModel
  {
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public string SortOrder { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
  }

  public class UserListViewModel
  {
    public List<UserProfileViewModel> Users { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public UserFilterViewModel Filter { get; set; } = new();
  }

  public class SetActiveStatusViewModel
  {
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public bool IsActive { get; set; }
  }

  public class AssignRoleViewModel
  {
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
  }

  public class RevokeRoleViewModel
  {
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
  }

  public class RemoveUserViewModel
  {
      [Required]
      public string UserId { get; set; } = string.Empty;
  }

  public class UserRolesViewModel
  {
    public string UserId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
  }

  public class LinkExternalLoginViewModel
  {
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
  }
}