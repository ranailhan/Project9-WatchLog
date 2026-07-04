using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WatchLog.MVC.Data;
using WatchLog.MVC.Models.ApiModels;
using WatchLog.MVC.Models.Entities;
using WatchLog.MVC.Models.ViewModels;

namespace WatchLog.MVC.Controllers;

public class MoviesController : Controller
{
    private readonly HttpClient _http;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MoviesController(IHttpClientFactory factory, AppDbContext db, UserManager<ApplicationUser> um)
    {
        _http        = factory.CreateClient("WatchLogAPI");
        _db          = db;
        _userManager = um;
    }

    // GET /Movies
    public async Task<IActionResult> Index(int? genreId)
    {
        var url    = genreId.HasValue ? $"api/movies/by-genre/{genreId}" : "api/movies";
        var json   = await _http.GetStringAsync(url);
        var movies = JsonConvert.DeserializeObject<List<MovieModel>>(json) ?? new();

        var genresJson = await _http.GetStringAsync("api/genres");
        var genres     = JsonConvert.DeserializeObject<List<GenreModel>>(genresJson) ?? new();

        ViewBag.Genres        = genres;
        ViewBag.SelectedGenre = genreId;
        return View(movies);
    }

    // GET /Movies/Detail/5
    public async Task<IActionResult> Detail(int id)
    {
        try
        {
            var response = await _http.GetAsync($"api/movies/{id}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                TempData["Error"] = $"API Hatası ({response.StatusCode}): Film detayları alınamadı. Lütfen API veritabanının (WatchLogDB) ve stored procedure'lerin hazır olduğundan emin olun.";
                return RedirectToAction(nameof(Index));
            }

            var json  = await response.Content.ReadAsStringAsync();
            var movie = JsonConvert.DeserializeObject<MovieDetailModel>(json);
            if (movie == null) return NotFound();

            // MVC DB'den puan, favori bilgisi
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var rating = await _db.Ratings.FirstOrDefaultAsync(
                    r => r.UserId == userId && r.ContentId == id && r.ContentType == "Movie" && r.IsActive);
                movie.UserRating = rating?.Score;

                movie.IsFavorited = await _db.Favorites.AnyAsync(
                    f => f.UserId == userId && f.ContentId == id && f.ContentType == "Movie" && f.IsActive);

                ViewBag.Watchlists = await _db.Watchlists
                    .Where(w => w.UserId == userId && w.IsActive)
                    .OrderBy(w => w.Name)
                    .ToListAsync();
            }

            var avg = await _db.Ratings
                .Where(r => r.ContentId == id && r.ContentType == "Movie" && r.IsActive)
                .AverageAsync(r => (double?)r.Score);
            movie.AverageRating = avg.HasValue ? Math.Round(avg.Value, 1) : null;
            movie.RatingCount   = await _db.Ratings.CountAsync(r => r.ContentId == id && r.ContentType == "Movie" && r.IsActive);

            // Yorumları çek
            var reviews = await _db.Reviews
                .Where(r => r.ContentId == id && r.ContentType == "Movie" && r.IsActive)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id, r.Text, r.CreatedAt, r.UserId,
                    UserName = r.User.DisplayName ?? r.User.Email
                })
                .ToListAsync();

            ViewBag.Reviews = reviews;
            ViewBag.UserId  = userId;
            return View(movie);
        }
        catch (Exception)
        {
            TempData["Error"] = "API bağlantı hatası. Lütfen API servisinin çalıştığından emin olun.";
            return RedirectToAction(nameof(Index));
        }
    }
}
