using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WatchLog.MVC.Data;
using WatchLog.MVC.Models.ApiModels;
using WatchLog.MVC.Models.Entities;
using WatchLog.MVC.Models.ViewModels;

namespace WatchLog.MVC.Controllers;

[Authorize]
public class FavoritesController : Controller
{
    private readonly HttpClient _http;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoritesController(IHttpClientFactory factory, AppDbContext db, UserManager<ApplicationUser> um)
    {
        _http        = factory.CreateClient("WatchLogAPI");
        _db          = db;
        _userManager = um;
    }

    // GET /Favorites
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var favs   = await _db.Favorites
            .Where(f => f.UserId == userId && f.IsActive)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var moviesJson = await _http.GetStringAsync("api/movies");
        var seriesJson = await _http.GetStringAsync("api/series");
        var allMovies  = JsonConvert.DeserializeObject<List<MovieModel>>(moviesJson) ?? new();
        var allSeries  = JsonConvert.DeserializeObject<List<SeriesModel>>(seriesJson) ?? new();

        var items = favs.Select(f =>
        {
            string title = "Bilinmiyor"; string? poster = null; int? year = null;
            if (f.ContentType == "Movie")
            {
                var m = allMovies.FirstOrDefault(x => x.Id == f.ContentId);
                if (m != null) { title = m.Title; poster = m.PosterUrl; year = m.ReleaseYear; }
            }
            else
            {
                var s = allSeries.FirstOrDefault(x => x.Id == f.ContentId);
                if (s != null) { title = s.Title; poster = s.PosterUrl; year = s.StartYear; }
            }
            return new FavoriteViewModel
            {
                Id = f.Id,
                ContentId = f.ContentId,
                ContentType = f.ContentType,
                Title = title,
                PosterUrl = poster,
                Year = year,
                CreatedAt = f.CreatedAt
            };
        }).ToList();

        return View(items);
    }


    // POST /Favorites/Toggle
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int contentId, string contentType, string returnUrl)
    {
        var userId   = _userManager.GetUserId(User)!;
        var existing = await _db.Favorites.FirstOrDefaultAsync(
            f => f.UserId == userId && f.ContentId == contentId && f.ContentType == contentType);

        if (existing == null)
        {
            _db.Favorites.Add(new Favorite { UserId = userId, ContentId = contentId, ContentType = contentType });
            TempData["Success"] = "Favorilere eklendi!";
        }
        else if (!existing.IsActive)
        {
            existing.IsActive   = true;
            TempData["Success"] = "Favorilere eklendi!";
        }
        else
        {
            existing.IsActive = false;
            TempData["Info"]  = "Favorilerden çıkarıldı.";
        }

        await _db.SaveChangesAsync();
        return Redirect(returnUrl);
    }
}

// ── Puanlama ─────────────────────────────────────────────────────────────────

[Authorize]
public class RatingsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public RatingsController(AppDbContext db, UserManager<ApplicationUser> um)
    {
        _db          = db;
        _userManager = um;
    }

    // POST /Ratings/Rate
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(int contentId, string contentType, int score, string returnUrl)
    {
        if (score < 1 || score > 10) return BadRequest();

        var userId   = _userManager.GetUserId(User)!;
        var existing = await _db.Ratings.FirstOrDefaultAsync(
            r => r.UserId == userId && r.ContentId == contentId && r.ContentType == contentType);

        if (existing == null)
            _db.Ratings.Add(new Rating
            {
                UserId      = userId,
                ContentId   = contentId,
                ContentType = contentType,
                Score       = score,
                IsActive    = true
            });
        else
        {
            existing.Score    = score;
            existing.IsActive = true;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Puanınız kaydedildi: {score}/10";
        return Redirect(returnUrl);
    }
}

// ── Yorumlar ─────────────────────────────────────────────────────────────────

[Authorize]
public class ReviewsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReviewsController(AppDbContext db, UserManager<ApplicationUser> um)
    {
        _db          = db;
        _userManager = um;
    }

    // POST /Reviews/Add
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int contentId, string contentType, string text, string returnUrl)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            TempData["Error"] = "Yorum boş olamaz.";
            return Redirect(returnUrl);
        }

        var userId = _userManager.GetUserId(User)!;
        _db.Reviews.Add(new Review
        {
            UserId      = userId,
            ContentId   = contentId,
            ContentType = contentType,
            Text        = text.Trim(),
            IsActive    = true
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Yorumunuz eklendi.";
        return Redirect(returnUrl);
    }

    // POST /Reviews/Delete/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string returnUrl)
    {
        var userId = _userManager.GetUserId(User)!;
        var review = await _db.Reviews.FindAsync(id);
        if (review != null && (review.UserId == userId || User.IsInRole("Admin")))
        {
            review.IsActive = false;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Yorum silindi.";
        }
        return Redirect(returnUrl);
    }
}
