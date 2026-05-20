using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Online_Booking_System.Models;
using Online_Booking_System.ViewModels;
using System.Security.Claims;
using Online_Booking_System.Contracts;

namespace Online_Booking_System.Controllers
{
  public class AccountController : Controller
  {
    private readonly IUserService _userService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
      IUserService userService,
      SignInManager<ApplicationUser> signInManager,
      UserManager<ApplicationUser> userManager)
    {
      _userService = userService;
      _signInManager = signInManager;
      _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
      return View();
    }

    [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (userId, token) = await _userService.RegisterAsync(model);
            if (userId == null)
            {
                ModelState.AddModelError("", "Registration failed. Email may already be in use.");
                return View(model);
            }

            TempData["Success"] = "Registration successful! Please check your email to confirm your account.";
            return RedirectToAction("Login");
        }

        [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string? userId = null, string? token = null)
    {
      userId ??= TempData["UserId"]?.ToString();
      token ??= TempData["ConfirmationToken"]?.ToString();

      // URL-decode the token if it's encoded
      if (!string.IsNullOrEmpty(token) && token.Contains("%"))
      {
        token = HttpUtility.UrlDecode(token);
      }

      if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
      {
        return RedirectToAction(nameof(Register));
      }

      // Auto-confirm when accessed via email link with valid parameters
      var model = new ConfirmEmailViewModel
      {
        UserId = userId,
        Token = token
      };

      var result = await _userService.ConfirmEmailAsync(model);
      if (result)
      {
        TempData["Success"] = "Email confirmed successfully. Please login.";
        return RedirectToAction(nameof(Login));
      }

      // If auto-confirm fails, show the form
      return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailViewModel model)
    {
      var result = await _userService.ConfirmEmailAsync(model);
      if (result)
      {
        TempData["Success"] = "Email confirmed successfully. Please login.";
        return RedirectToAction(nameof(Login));
      }

      ModelState.AddModelError("", "Invalid confirmation token.");
      return View(model);
    }

        [HttpGet]
        public IActionResult ResendConfirmation()
        {
            return View(new ResendConfirmationViewModel());
        }

        [HttpPost]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var token = await _userService.ResendConfirmationEmailAsync(model.Email);
      if (token != null)
      {
        TempData["ConfirmationToken"] = token;
        TempData["UserId"] = await _userService.GetWithProfileAsync("") is null ? "" : "";
        TempData["Success"] = "Confirmation email sent.";
      }

      return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
      ViewBag.ReturnUrl = returnUrl;
      return View();
    }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _userService.LoginAsync(model);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error ?? "Invalid email or password.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
    public async Task<IActionResult> Logout()
    {
      await _userService.LogoutAsync();
      return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(SendPasswordResetViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      await _userService.SendPasswordResetAsync(model.Email);
      TempData["Success"] = "If the email exists, a password reset link has been sent.";
      return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string userId, string token)
    {
      if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
      {
        return RedirectToAction(nameof(Login));
      }

      var model = new ResetPasswordViewModel
      {
        Email = userId,
        Token = token
      };

      return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var result = await _userService.ResetPasswordAsync(model);
      if (!result)
      {
        ModelState.AddModelError("", "Invalid reset token or email.");
        return View(model);
      }

      TempData["Success"] = "Password reset successfully. Please login.";
      return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
      var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      if (string.IsNullOrEmpty(userId))
      {
        return RedirectToAction(nameof(Login));
      }

      var profile = await _userService.GetWithProfileAsync(userId);
      if (profile == null)
      {
        return NotFound();
      }

      return View(profile);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
      var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      if (string.IsNullOrEmpty(userId))
      {
        return RedirectToAction(nameof(Login));
      }

      var profile = await _userService.GetWithProfileAsync(userId);
      if (profile == null)
      {
        return NotFound();
      }

      var model = new EditProfileViewModel
      {
        FirstName = profile.FirstName,
        LastName = profile.LastName,
        PhoneNumber = profile.PhoneNumber,
        AvatarUrl = profile.AvatarUrl,
        Bio = profile.Bio
      };

      return View(model);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      if (string.IsNullOrEmpty(userId))
      {
        return RedirectToAction(nameof(Login));
      }

      var result = await _userService.UpdateProfileAsync(userId, model);
      if (!result)
      {
        ModelState.AddModelError("", "Failed to update profile.");
        return View(model);
      }

      TempData["Success"] = "Profile updated successfully.";
      return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
      return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      if (string.IsNullOrEmpty(userId))
      {
        return RedirectToAction(nameof(Login));
      }

      var (success, error) = await _userService.ChangePasswordAsync(userId, model);
      if (!success)
      {
        ModelState.AddModelError("", error ?? "Failed to change password.");
        return View(model);
      }

      TempData["Success"] = "Password changed successfully.";
      return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Users(UserFilterViewModel filter)
    {
      var model = await _userService.GetUsersAsync(filter);
      return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetActiveStatus(string userId)
    {
      var user = await _userService.GetWithProfileAsync(userId);
      if (user == null)
      {
        return NotFound();
      }

      var model = new SetActiveStatusViewModel
      {
        UserId = userId,
        IsActive = !user.IsActive
      };

      return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetActiveStatus(SetActiveStatusViewModel model)
    {
      var result = await _userService.SetActiveStatusAsync(model.UserId, model.IsActive);
      if (!result)
      {
        ModelState.AddModelError("", "Failed to update status.");
        return View(model);
      }

      TempData["Success"] = $"User {(model.IsActive ? "activated" : "deactivated")} successfully.";
      return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole(string userId)
    {
      var user = await _userService.GetWithProfileAsync(userId);
      if (user == null)
      {
        return NotFound();
      }

      var model = new AssignRoleViewModel
      {
        UserId = userId,
        Role = ""
      };

      ViewBag.User = user;
      return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var result = await _userService.AssignRoleAsync(model.UserId, model.Role);
      if (!result)
      {
        ModelState.AddModelError("", "Failed to assign role.");
        return View(model);
      }

      TempData["Success"] = "Role assigned successfully.";
      return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RevokeRole(string userId, string role)
    {
      var user = await _userService.GetWithProfileAsync(userId);
      if (user == null)
      {
        return NotFound();
      }

      ViewBag.User = user;
      ViewBag.Role = role;
      
      var model = new RevokeRoleViewModel
      {
          UserId = userId,
          Role = role
      };

      return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeRole(RevokeRoleViewModel model)
    {
      var result = await _userService.RevokeRoleAsync(model.UserId, model.Role);
      if (!result)
      {
        TempData["Error"] = "Failed to revoke role. You cannot revoke your own Admin role if it is your only role.";
      }
      else
      {
        TempData["Success"] = $"Role '{model.Role}' revoked successfully.";
      }

      return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveUser(string userId)
    {
      var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      if (currentUserId == userId)
      {
          TempData["Error"] = "You cannot remove your own account.";
          return RedirectToAction(nameof(Users));
      }

      var user = await _userService.GetWithProfileAsync(userId);
      if (user == null)
      {
        return NotFound();
      }

      ViewBag.User = user;
      return View(new RemoveUserViewModel { UserId = userId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveUser(RemoveUserViewModel model)
    {
      var result = await _userService.RemoveUserAsync(model.UserId);
      if (!result)
      {
        TempData["Error"] = "Failed to remove user. You cannot remove your own account.";
      }
      else
      {
        TempData["Success"] = "User removed successfully.";
      }

      return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
      var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
      var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
      return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
    {
      var info = await _signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        return RedirectToAction(nameof(Login));
      }

      var email = info.Principal.FindFirstValue(ClaimTypes.Email);
      var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "External";
      var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "User";

      var user = await _userManager.FindByEmailAsync(email ?? "");

      if (user != null)
      {
        var logins = await _userManager.GetLoginsAsync(user);
        var existingLogin = logins.FirstOrDefault(l => l.LoginProvider == info.LoginProvider);

        if (existingLogin == null)
        {
            await _userManager.AddLoginAsync(user, info);
        }
        
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToLocal(returnUrl);
      }

      // New user registration via External Provider
      var newUser = new ApplicationUser
      {
          Email = email,
          UserName = email,
          FirstName = firstName,
          LastName = lastName,
          IsActive = true,
          EmailConfirmed = true, // Trusted from Google
          CreatedAt = DateTime.UtcNow
      };

      var result = await _userManager.CreateAsync(newUser);
      if (result.Succeeded)
      {
          await _userManager.AddToRoleAsync(newUser, "User");
          await _userManager.AddLoginAsync(newUser, info);
          await _signInManager.SignInAsync(newUser, isPersistent: false);
          
          TempData["Success"] = $"Successfully registered and logged in with {info.LoginProvider}.";
          return RedirectToLocal(returnUrl);
      }

      TempData["Error"] = "Failed to create account from external provider.";
      return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> LinkExternalLogin(LinkExternalLoginViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      var info = await _signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        return RedirectToAction(nameof(Login));
      }

      var user = await _userManager.FindByEmailAsync(model.Email);
      if (user == null)
      {
        ModelState.AddModelError("", "User not found. Please login with local account first.");
        return View(model);
      }

      var result = await _userManager.AddLoginAsync(user, info);
      if (!result.Succeeded)
      {
        ModelState.AddModelError("", "Failed to link account.");
        return View(model);
      }

      await _signInManager.SignInAsync(user, isPersistent: false);
      TempData["Success"] = $"Successfully linked {info.LoginProvider} account.";
      return RedirectToAction(nameof(Profile));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
      if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
      {
        return Redirect(returnUrl);
      }
      return RedirectToAction("Index", "Home");
    }
  }
}