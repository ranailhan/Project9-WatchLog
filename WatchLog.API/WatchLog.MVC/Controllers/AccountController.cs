using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WatchLog.MVC.Data;
using WatchLog.MVC.Models.Entities;
using WatchLog.MVC.Models.ViewModels;

namespace WatchLog.MVC.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser>  _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly AppDbContext _db;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        AppDbContext db)
    {
        _userManager  = userManager;
        _signInManager = signInManager;
        _db  = db;
    }

    // GET /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST /Account/Login
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user != null && !user.IsActive)
        {
            ModelState.AddModelError("", "Hesabınız pasif hale getirilmiştir. Lütfen sistem yöneticisiyle iletişime geçin.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Geçersiz e-posta veya şifre.");
        return View(model);
    }

    // GET /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View();
    }

    // POST /Account/Register
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName    = model.Email,
            Email       = model.Email,
            DisplayName = model.DisplayName,
            IsActive    = true,
            CreatedAt   = DateTime.Now
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);
        return View(model);
    }

    // POST /Account/Logout
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // GET /Account/Profile
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        var vm = new ProfileViewModel
        {
            UserId          = user.Id,
            DisplayName     = user.DisplayName,
            Email           = user.Email,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            Bio             = user.Bio,
            WatchlistCount  = await _db.Watchlists.CountAsync(w => w.UserId == user.Id && w.IsActive),
            FavoriteCount   = await _db.Favorites.CountAsync(f => f.UserId == user.Id && f.IsActive),
            ReviewCount     = await _db.Reviews.CountAsync(r => r.UserId == user.Id && r.IsActive),
            RatingCount     = await _db.Ratings.CountAsync(r => r.UserId == user.Id && r.IsActive)
        };

        return View(vm);
    }

    // GET /Account/AccessDenied
    public IActionResult AccessDenied() => View();
}
