using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Online_Booking_System.Contracts;
using Online_Booking_System.Data;
using Online_Booking_System.Models;
using Online_Booking_System.ViewModels;

namespace Online_Booking_System.Services
{
  public class UserService : IUserService
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context;

    public UserService(
      UserManager<ApplicationUser> userManager,
      SignInManager<ApplicationUser> signInManager,
      IEmailService emailService,
      AppDbContext context)
    {
      _userManager = userManager;
      _signInManager = signInManager;
      _emailService = emailService;
      _context = context;
    }

    public async Task<(string? UserId, string? Token)> RegisterAsync(RegisterViewModel model)
    {
      var existingUser = await _userManager.FindByEmailAsync(model.Email);
      if (existingUser != null)
      {
        return (null, null);
      }

      var user = new ApplicationUser
      {
        Email = model.Email,
        UserName = model.Email,
        FirstName = model.FirstName,
        LastName = model.LastName,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
      };

      var result = await _userManager.CreateAsync(user, model.Password);
      if (!result.Succeeded)
      {
        return (null, null);
      }

      await _userManager.AddToRoleAsync(user, "User");

      var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      await _emailService.SendConfirmationEmailAsync(model.Email, user.Id, token, user.FullName);

      return (user.Id, token);
    }

    public async Task<bool> ConfirmEmailAsync(ConfirmEmailViewModel model)
    {
      var user = await _userManager.FindByIdAsync(model.UserId);
      if (user == null)
      {
        return false;
      }

      var result = await _userManager.ConfirmEmailAsync(user, model.Token);
      return result.Succeeded;
    }

    public async Task<string?> ResendConfirmationEmailAsync(string email)
    {
      var user = await _userManager.FindByEmailAsync(email);
      if (user == null)
      {
        return null;
      }

      if (user.EmailConfirmed)
      {
        return null;
      }

      var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      await _emailService.SendConfirmationEmailAsync(email, user.Id, token, user.FullName);

      return token;
    }

    public async Task<LoginViewModel> LoginAsync(LoginViewModel model)
    {
      var user = await _userManager.FindByEmailAsync(model.Email);
      if (user == null)
      {
        model.Password = "ERROR";
        return model;
      }

      if (!user.IsActive)
      {
        model.Password = "ERROR";
        return model;
      }

      var result = await _signInManager.PasswordSignInAsync(
        user,
        model.Password,
        isPersistent: false,
        lockoutOnFailure: true);

      if (!result.Succeeded)
      {
        model.Password = "ERROR";
        return model;
      }

      user.LastLoginAt = DateTime.UtcNow;
      await _userManager.UpdateAsync(user);

      model.UserId = user.Id;

      model.IsSuccess = true;
      return model;
    }

    public async Task LogoutAsync()
    {
      await _signInManager.SignOutAsync();
    }

    public async Task<bool> SendPasswordResetAsync(string email)
    {
      var user = await _userManager.FindByEmailAsync(email);
      if (user == null)
      {
        return true;
      }

      var token = await _userManager.GeneratePasswordResetTokenAsync(user);
      await _emailService.SendPasswordResetEmailAsync(email, token, user.FullName);

      return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordViewModel model)
    {
      var user = await _userManager.FindByEmailAsync(model.Email);
      if (user == null)
      {
        return false;
      }

      var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
      return result.Succeeded;
    }

    public async Task<UserProfileViewModel?> GetWithProfileAsync(string userId)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
      {
        return null;
      }

      var roles = await _userManager.GetRolesAsync(user);

      return new UserProfileViewModel
      {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        PhoneNumber = user.PhoneNumber,
        AvatarUrl = user.AvatarUrl,
        Bio = user.Bio,
        IsActive = user.IsActive,
        EmailConfirmed = user.EmailConfirmed,
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt,
        Roles = roles.ToList()
      };
    }

    public async Task<bool> UpdateProfileAsync(string userId, EditProfileViewModel model)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
      {
        return false;
      }

      if (!string.IsNullOrEmpty(model.FirstName))
      {
        user.FirstName = model.FirstName;
      }

      if (!string.IsNullOrEmpty(model.LastName))
      {
        user.LastName = model.LastName;
      }

      user.PhoneNumber = model.PhoneNumber;
      user.AvatarUrl = model.AvatarUrl;
      user.Bio = model.Bio;

      var result = await _userManager.UpdateAsync(user);
      return result.Succeeded;
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
      {
        return (false, "User not found");
      }

      var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

      if (!result.Succeeded)
      {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return (false, errors);
      }

      await _signInManager.RefreshSignInAsync(user);

      return (true, null);
    }

    public async Task<UserListViewModel> GetUsersAsync(UserFilterViewModel filter)
    {
      var query = _context.Users.AsQueryable();

      if (!string.IsNullOrEmpty(filter.Search))
      {
        var search = filter.Search.ToLower();
        query = query.Where(u =>
          u.FirstName.ToLower().Contains(search) ||
          u.LastName.ToLower().Contains(search) ||
          u.Email!.ToLower().Contains(search));
      }

      if (!string.IsNullOrEmpty(filter.Role))
      {
        var roleUserIds = _context.UserRoles
          .Where(ur => _context.Roles.Any(r => r.Name == filter.Role && r.Id == ur.RoleId))
          .Select(ur => ur.UserId);

        query = query.Where(u => roleUserIds.Contains(u.Id));
      }

      if (filter.IsActive.HasValue)
      {
        query = query.Where(u => u.IsActive == filter.IsActive.Value);
      }

      var totalCount = await query.CountAsync();

      // Sorting
      bool isDesc = filter.SortOrder.ToLower() == "desc";
      query = filter.SortBy.ToLower() switch
      {
          "email" => isDesc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
          "firstname" => isDesc ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
          "lastname" => isDesc ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
          "createdat" => isDesc ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
          _ => query.OrderByDescending(u => u.CreatedAt)
      };

      var users = await query
        .Skip((filter.Page - 1) * filter.PageSize)
        .Take(filter.PageSize)
        .ToListAsync();

      var userProfiles = new List<UserProfileViewModel>();
      foreach (var user in users)
      {
        var roles = await _userManager.GetRolesAsync(user);
        userProfiles.Add(new UserProfileViewModel
        {
          Id = user.Id,
          Email = user.Email ?? string.Empty,
          FirstName = user.FirstName,
          LastName = user.LastName,
          PhoneNumber = user.PhoneNumber,
          AvatarUrl = user.AvatarUrl,
          Bio = user.Bio,
          IsActive = user.IsActive,
          EmailConfirmed = user.EmailConfirmed,
          CreatedAt = user.CreatedAt,
          LastLoginAt = user.LastLoginAt,
          Roles = roles.ToList()
        });
      }

      return new UserListViewModel
      {
        Users = userProfiles,
        TotalCount = totalCount,
        Page = filter.Page,
        PageSize = filter.PageSize,
        Filter = filter
      };
    }

    public async Task<bool> SetActiveStatusAsync(string userId, bool isActive)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
      {
        return false;
      }

      user.IsActive = isActive;
      await _userManager.UpdateAsync(user);

      await _userManager.UpdateSecurityStampAsync(user);

      if (!isActive)
      {
        await _signInManager.SignOutAsync();
      }

      return true;
    }

    public async Task<bool> AssignRoleAsync(string userId, string role)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
      {
        return false;
      }

      var result = await _userManager.AddToRoleAsync(user, role);
      if (!result.Succeeded)
      {
        return false;
      }

      await _userManager.UpdateSecurityStampAsync(user);

      return true;
    }

    public async Task<bool> RevokeRoleAsync(string userId, string role)
    {
      var currentUserId = _signInManager.Context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
      if (currentUserId == userId && role == "Admin")
      {
        var existUser = await _userManager.FindByIdAsync(userId);
        
        if (existUser == null) return false;
        
        var currentRoles = await _userManager.GetRolesAsync(existUser);
        if (currentRoles.Contains("Admin") && currentRoles.Count == 1)
        {
          return false;
        }
      }

      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
      {
        return false;
      }

      var result = await _userManager.RemoveFromRoleAsync(user, role);
      if (!result.Succeeded)
      {
        return false;
      }

      await _userManager.UpdateSecurityStampAsync(user);

      return true;
    }

    public async Task<bool> RemoveUserAsync(string userId)
    {
        var currentUserId = _signInManager.Context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == userId)
        {
            return false; // Cannot delete self
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
      var user = await _userManager.FindByIdAsync(userId);
      if (user == null)
      {
        return [];
      }

      var roles = await _userManager.GetRolesAsync(user);
      return roles.ToList();
    }
  }
}