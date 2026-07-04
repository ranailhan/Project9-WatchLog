using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using WatchLog.MVC.Data;
using WatchLog.MVC.Models.ApiModels;
using WatchLog.MVC.Models.Entities;
using WatchLog.MVC.Models.ViewModels;

namespace WatchLog.MVC.Controllers.Admin;

// ── Dashboard ─────────────────────────────────────────────────────────────────

[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly HttpClient _http;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(IHttpClientFactory factory, AppDbContext db, UserManager<ApplicationUser> um)
    {
        _http        = factory.CreateClient("WatchLogAPI");
        _db          = db;
        _userManager = um;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Dashboard";

        var moviesJson = await _http.GetStringAsync("api/movies");
        var seriesJson = await _http.GetStringAsync("api/series");
        var actorsJson = await _http.GetStringAsync("api/actors");
        var genresJson = await _http.GetStringAsync("api/genres");

        var movies = JsonConvert.DeserializeObject<List<MovieModel>>(moviesJson) ?? new();
        var series = JsonConvert.DeserializeObject<List<SeriesModel>>(seriesJson) ?? new();
        var actors = JsonConvert.DeserializeObject<List<ActorModel>>(actorsJson) ?? new();
        var genres = JsonConvert.DeserializeObject<List<GenreModel>>(genresJson) ?? new();

        var vm = new AdminDashboardViewModel
        {
            MovieCount    = movies.Count,
            SeriesCount   = series.Count,
            ActorCount    = actors.Count,
            GenreCount    = genres.Count,
            UserCount     = await _db.Users.CountAsync(),
            ReviewCount   = await _db.Reviews.CountAsync(r => r.IsActive),
            FavoriteCount = await _db.Favorites.CountAsync(f => f.IsActive)
        };
        return View(vm);
    }
}

// ── Admin Movies ──────────────────────────────────────────────────────────────

[Authorize(Roles = "Admin")]
public class AdminMoviesController : Controller
{
    private readonly HttpClient _http;

    public AdminMoviesController(IHttpClientFactory factory)
        => _http = factory.CreateClient("WatchLogAPI");

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Filmler";
        var json   = await _http.GetStringAsync("api/movies");
        var movies = JsonConvert.DeserializeObject<List<MovieModel>>(json) ?? new();
        return View(movies);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Yeni Film Ekle";
        ViewBag.Genres = JsonConvert.DeserializeObject<List<GenreModel>>(await _http.GetStringAsync("api/genres")) ?? new();
        ViewBag.Actors = JsonConvert.DeserializeObject<List<ActorModel>>(await _http.GetStringAsync("api/actors")) ?? new();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string title, string? description, int? releaseYear, int? duration,
        string? posterUrl, string? trailerUrl, string? director, string? country, string? language,
        int[]? genreIds, int[]? actorIds)
    {
        var dto  = new { Title = title, Description = description, ReleaseYear = releaseYear, Duration = duration,
                         PosterUrl = posterUrl, TrailerUrl = trailerUrl, Director = director,
                         Country = country, Language = language,
                         GenreIds = genreIds ?? Array.Empty<int>(),
                         ActorIds = actorIds ?? Array.Empty<int>() };
        var body = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
        var res  = await _http.PostAsync("api/movies", body);

        TempData[res.IsSuccessStatusCode ? "Success" : "Error"] =
            res.IsSuccessStatusCode ? "Film başarıyla eklendi." : "Film eklenirken hata oluştu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Film Düzenle";
        var res = await _http.GetAsync($"api/movies/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();
        var movie = JsonConvert.DeserializeObject<MovieDetailModel>(await res.Content.ReadAsStringAsync());
        ViewBag.Genres = JsonConvert.DeserializeObject<List<GenreModel>>(await _http.GetStringAsync("api/genres")) ?? new();
        ViewBag.Actors = JsonConvert.DeserializeObject<List<ActorModel>>(await _http.GetStringAsync("api/actors")) ?? new();
        return View(movie);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, string title, string? description, int? releaseYear, int? duration,
        string? posterUrl, string? trailerUrl, string? director, string? country, string? language,
        int[]? genreIds, int[]? actorIds)
    {
        var dto  = new { Id = id, Title = title, Description = description, ReleaseYear = releaseYear, Duration = duration,
                         PosterUrl = posterUrl, TrailerUrl = trailerUrl, Director = director,
                         Country = country, Language = language,
                         GenreIds = genreIds ?? Array.Empty<int>(),
                         ActorIds = actorIds ?? Array.Empty<int>() };
        var body = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
        var res  = await _http.PutAsync($"api/movies/{id}", body);

        TempData[res.IsSuccessStatusCode ? "Success" : "Error"] =
            res.IsSuccessStatusCode ? "Film güncellendi." : "Güncelleme hatası.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _http.DeleteAsync($"api/movies/{id}");
        TempData["Success"] = "Film silindi.";
        return RedirectToAction(nameof(Index));
    }
}

// ── Admin Series ──────────────────────────────────────────────────────────────

[Authorize(Roles = "Admin")]
public class AdminSeriesController : Controller
{
    private readonly HttpClient _http;

    public AdminSeriesController(IHttpClientFactory factory)
        => _http = factory.CreateClient("WatchLogAPI");

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Diziler";
        var json   = await _http.GetStringAsync("api/series");
        var series = JsonConvert.DeserializeObject<List<SeriesModel>>(json) ?? new();
        return View(series);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Yeni Dizi Ekle";
        ViewBag.Genres = JsonConvert.DeserializeObject<List<GenreModel>>(await _http.GetStringAsync("api/genres")) ?? new();
        ViewBag.Actors = JsonConvert.DeserializeObject<List<ActorModel>>(await _http.GetStringAsync("api/actors")) ?? new();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string title, string? description, int? startYear, int? endYear,
        int? seasonCount, int? episodeCount, string? posterUrl, string? trailerUrl,
        string? director, string? country, string? language, int[]? genreIds, int[]? actorIds)
    {
        var dto  = new { Title = title, Description = description, StartYear = startYear, EndYear = endYear,
                         SeasonCount = seasonCount, EpisodeCount = episodeCount,
                         PosterUrl = posterUrl, TrailerUrl = trailerUrl, Director = director,
                         Country = country, Language = language,
                         GenreIds = genreIds ?? Array.Empty<int>(),
                         ActorIds = actorIds ?? Array.Empty<int>() };
        var body = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
        var res  = await _http.PostAsync("api/series", body);

        TempData[res.IsSuccessStatusCode ? "Success" : "Error"] =
            res.IsSuccessStatusCode ? "Dizi eklendi." : "Hata oluştu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Dizi Düzenle";
        var res = await _http.GetAsync($"api/series/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();
        var series = JsonConvert.DeserializeObject<SeriesDetailModel>(await res.Content.ReadAsStringAsync());
        ViewBag.Genres = JsonConvert.DeserializeObject<List<GenreModel>>(await _http.GetStringAsync("api/genres")) ?? new();
        ViewBag.Actors = JsonConvert.DeserializeObject<List<ActorModel>>(await _http.GetStringAsync("api/actors")) ?? new();
        return View(series);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, string title, string? description, int? startYear, int? endYear,
        int? seasonCount, int? episodeCount, string? posterUrl, string? trailerUrl,
        string? director, string? country, string? language, int[]? genreIds, int[]? actorIds)
    {
        var dto  = new { Id = id, Title = title, Description = description, StartYear = startYear, EndYear = endYear,
                         SeasonCount = seasonCount, EpisodeCount = episodeCount,
                         PosterUrl = posterUrl, TrailerUrl = trailerUrl, Director = director,
                         Country = country, Language = language,
                         GenreIds = genreIds ?? Array.Empty<int>(),
                         ActorIds = actorIds ?? Array.Empty<int>() };
        var body = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
        var res  = await _http.PutAsync($"api/series/{id}", body);

        TempData[res.IsSuccessStatusCode ? "Success" : "Error"] =
            res.IsSuccessStatusCode ? "Dizi güncellendi." : "Hata oluştu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _http.DeleteAsync($"api/series/{id}");
        TempData["Success"] = "Dizi silindi.";
        return RedirectToAction(nameof(Index));
    }
}

// ── Admin Actors ──────────────────────────────────────────────────────────────

[Authorize(Roles = "Admin")]
public class AdminActorsController : Controller
{
    private readonly HttpClient _http;

    public AdminActorsController(IHttpClientFactory factory)
        => _http = factory.CreateClient("WatchLogAPI");

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Oyuncular";
        var json   = await _http.GetStringAsync("api/actors");
        var actors = JsonConvert.DeserializeObject<List<ActorModel>>(json) ?? new();
        return View(actors);
    }

    public IActionResult Create() { ViewData["Title"] = "Yeni Oyuncu Ekle"; return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string fullName, DateTime? birthDate, string? biography, string? photoUrl, string? nationality)
    {
        var dto  = new { FullName = fullName, BirthDate = birthDate, Biography = biography, PhotoUrl = photoUrl, Nationality = nationality };
        var body = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
        var res  = await _http.PostAsync("api/actors", body);

        TempData[res.IsSuccessStatusCode ? "Success" : "Error"] =
            res.IsSuccessStatusCode ? "Oyuncu eklendi." : "Hata oluştu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Oyuncu Düzenle";
        var res = await _http.GetAsync($"api/actors/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();
        var actor = JsonConvert.DeserializeObject<ActorModel>(await res.Content.ReadAsStringAsync());
        return View(actor);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id, string fullName, DateTime? birthDate, string? biography, string? photoUrl, string? nationality)
    {
        var dto  = new { Id = id, FullName = fullName, BirthDate = birthDate, Biography = biography, PhotoUrl = photoUrl, Nationality = nationality };
        var body = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
        var res  = await _http.PutAsync($"api/actors/{id}", body);

        TempData[res.IsSuccessStatusCode ? "Success" : "Error"] =
            res.IsSuccessStatusCode ? "Oyuncu güncellendi." : "Hata oluştu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _http.DeleteAsync($"api/actors/{id}");
        TempData["Success"] = "Oyuncu silindi.";
        return RedirectToAction(nameof(Index));
    }
}

// ── Admin Genres ──────────────────────────────────────────────────────────────

[Authorize(Roles = "Admin")]
public class AdminGenresController : Controller
{
    private readonly HttpClient _http;

    public AdminGenresController(IHttpClientFactory factory)
        => _http = factory.CreateClient("WatchLogAPI");

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Türler";
        var json   = await _http.GetStringAsync("api/genres");
        var genres = JsonConvert.DeserializeObject<List<GenreModel>>(json) ?? new();
        return View(genres);
    }

    public IActionResult Create() { ViewData["Title"] = "Yeni Tür Ekle"; return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        var body = new StringContent(JsonConvert.SerializeObject(new { Name = name }), Encoding.UTF8, "application/json");
        var res  = await _http.PostAsync("api/genres", body);
        TempData[res.IsSuccessStatusCode ? "Success" : "Error"] =
            res.IsSuccessStatusCode ? "Tür eklendi." : "Hata oluştu.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Tür Düzenle";
        var res = await _http.GetAsync($"api/genres/{id}");
        if (!res.IsSuccessStatusCode) return NotFound();
        var genre = JsonConvert.DeserializeObject<GenreModel>(await res.Content.ReadAsStringAsync());
        return View(genre);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string name)
    {
        var body = new StringContent(JsonConvert.SerializeObject(new { Id = id, Name = name }), Encoding.UTF8, "application/json");
        var res  = await _http.PutAsync($"api/genres/{id}", body);
        TempData[res.IsSuccessStatusCode ? "Success" : "Error"] =
            res.IsSuccessStatusCode ? "Tür güncellendi." : "Hata oluştu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _http.DeleteAsync($"api/genres/{id}");
        TempData["Success"] = "Tür silindi.";
        return RedirectToAction(nameof(Index));
    }
}

// ── Admin Users ───────────────────────────────────────────────────────────────

[Authorize(Roles = "Admin")]
public class AdminUsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _db;

    public AdminUsersController(UserManager<ApplicationUser> um, AppDbContext db)
    {
        _userManager = um;
        _db          = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Kullanıcılar";
        var users = _userManager.Users.OrderByDescending(u => u.CreatedAt).ToList();
        var vms   = new List<AdminUserViewModel>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            vms.Add(new AdminUserViewModel
            {
                Id          = u.Id,
                DisplayName = u.DisplayName,
                Email       = u.Email,
                IsActive    = u.IsActive,
                Role        = roles.FirstOrDefault() ?? "User",
                CreatedAt   = u.CreatedAt
            });
        }
        return View(vms);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null) { user.IsActive = false; await _userManager.UpdateAsync(user); }
        TempData["Success"] = "Kullanıcı pasif hale getirildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null) { user.IsActive = true; await _userManager.UpdateAsync(user); }
        TempData["Success"] = "Kullanıcı tekrar aktif edildi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var review = await _db.Reviews.FindAsync(reviewId);
        if (review != null) { review.IsActive = false; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Yorum silindi.";
        return RedirectToAction(nameof(Index));
    }
}
