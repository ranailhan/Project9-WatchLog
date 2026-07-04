using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WatchLog.MVC.Data;
using WatchLog.MVC.Models.ApiModels;
using WatchLog.MVC.Models.Entities;
using WatchLog.MVC.Models.ViewModels;
using QRCoder;

namespace WatchLog.MVC.Controllers;

[Authorize]
public class WatchlistController : Controller
{
    private readonly HttpClient _http;
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public WatchlistController(IHttpClientFactory factory, AppDbContext db, UserManager<ApplicationUser> um, IConfiguration config)
    {
        _http        = factory.CreateClient("WatchLogAPI");
        _db          = db;
        _userManager = um;
        _config      = config;
    }

    // GET /Watchlist
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var lists  = await _db.Watchlists
            .Where(w => w.UserId == userId && w.IsActive)
            .Include(w => w.Items)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
        return View(lists);
    }

    // GET /Watchlist/Create
    public IActionResult Create() => View();

    // POST /Watchlist/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWatchlistViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var userId = _userManager.GetUserId(User)!;
        _db.Watchlists.Add(new Watchlist
        {
            UserId      = userId,
            Name        = vm.Name,
            Description = vm.Description,
            IsPublic    = vm.IsPublic
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Liste oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Watchlist/Detail/5
    [AllowAnonymous]
    public async Task<IActionResult> Detail(int id)
    {
        var userId = _userManager.GetUserId(User);
        var list   = await _db.Watchlists
            .Include(w => w.Items.Where(i => i.IsActive))
            .FirstOrDefaultAsync(w => w.Id == id && w.IsActive && (w.UserId == userId || w.IsPublic));

        if (list == null) return NotFound();

        List<MovieModel> allMovies = new();
        List<SeriesModel> allSeries = new();

        try
        {
            // API'den içerik bilgilerini çek
            var moviesJson = await _http.GetStringAsync("api/movies");
            var seriesJson = await _http.GetStringAsync("api/series");
            allMovies  = JsonConvert.DeserializeObject<List<MovieModel>>(moviesJson) ?? new();
            allSeries  = JsonConvert.DeserializeObject<List<SeriesModel>>(seriesJson) ?? new();
        }
        catch (Exception)
        {
            TempData["Error"] = "API bağlantı hatası. İzleme listesi içerikleri tam yüklenemedi. Lütfen API servisinin çalıştığından emin olun.";
        }

        var vm = new WatchlistDetailViewModel
        {
            Id          = list.Id,
            UserId      = list.UserId,
            Name        = list.Name,
            Description = list.Description,
            IsPublic    = list.IsPublic,
            CreatedAt   = list.CreatedAt,
            Items       = list.Items.Select(item =>
            {
                string title = "Bilinmiyor"; string? poster = null; int? year = null;

                if (item.ContentType == "Movie")
                {
                    var m = allMovies.FirstOrDefault(x => x.Id == item.ContentId);
                    if (m != null) { title = m.Title; poster = m.PosterUrl; year = m.ReleaseYear; }
                }
                else
                {
                    var s = allSeries.FirstOrDefault(x => x.Id == item.ContentId);
                    if (s != null) { title = s.Title; poster = s.PosterUrl; year = s.StartYear; }
                }

                return new WatchlistItemViewModel
                {
                    Id = item.Id, ContentId = item.ContentId, ContentType = item.ContentType,
                    Title = title, PosterUrl = poster, Year = year, AddedAt = item.AddedAt
                };
            }).ToList()
        };

        var pattern = _config["QrSettings:ShareUrlPattern"] ?? "https://watchlog.com/watchlist/{0}";
        ViewBag.ShareUrl = pattern.Contains("{0}") ? string.Format(pattern, id) : pattern;

        return View(vm);
    }

    // POST /Watchlist/AddItem
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(int watchlistId, int contentId, string contentType, string returnUrl)
    {
        var userId = _userManager.GetUserId(User)!;
        var list   = await _db.Watchlists.FirstOrDefaultAsync(w => w.Id == watchlistId && w.UserId == userId && w.IsActive);
        if (list == null) return NotFound();

        var existing = await _db.WatchlistItems.FirstOrDefaultAsync(
            i => i.WatchlistId == watchlistId && i.ContentId == contentId && i.ContentType == contentType);

        if (existing == null)
        {
            _db.WatchlistItems.Add(new WatchlistItem
            {
                WatchlistId = watchlistId,
                ContentId   = contentId,
                ContentType = contentType,
                IsActive    = true
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "İçerik listeye eklendi.";
        }
        else if (!existing.IsActive)
        {
            existing.IsActive = true;
            existing.AddedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            TempData["Success"] = "İçerik listeye eklendi.";
        }
        else
        {
            TempData["Info"] = "Bu içerik zaten listede mevcut.";
        }

        return Redirect(returnUrl);
    }

    // POST /Watchlist/RemoveItem/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int id, int watchlistId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var isOwner = await _db.Watchlists.AnyAsync(w => w.Id == watchlistId && w.UserId == userId && w.IsActive);
        if (!isOwner) return Forbid();

        var item = await _db.WatchlistItems.FirstOrDefaultAsync(i => i.Id == id && i.WatchlistId == watchlistId);
        if (item != null) { item.IsActive = false; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Detail), new { id = watchlistId });
    }

    // POST /Watchlist/Delete/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var list   = await _db.Watchlists.FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId && w.IsActive);
        if (list != null) { list.IsActive = false; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Liste silindi.";
        return RedirectToAction(nameof(Index));
    }

    // GET /Watchlist/GetQrCode/{id}
    [AllowAnonymous]
    public IActionResult GetQrCode(int id)
    {
        var watchlist = _db.Watchlists.FirstOrDefault(w => w.Id == id && w.IsActive);
        if (watchlist == null)
            return NotFound();

        var pattern = _config["QrSettings:ShareUrlPattern"] ?? "https://watchlog.com/watchlist/{0}";
        string detailUrl = pattern.Contains("{0}") ? string.Format(pattern, id) : pattern;

        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(detailUrl, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            return File(qrCodeImage, "image/png");
        }
    }
}
