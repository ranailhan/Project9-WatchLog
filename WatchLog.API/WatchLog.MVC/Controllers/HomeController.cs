using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WatchLog.MVC.Models.ApiModels;

namespace WatchLog.MVC.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _http;

    public HomeController(IHttpClientFactory factory)
        => _http = factory.CreateClient("WatchLogAPI");

    public async Task<IActionResult> Index()
    {
        var moviesJson = await _http.GetStringAsync("api/movies");
        var seriesJson = await _http.GetStringAsync("api/series");
        var genresJson = await _http.GetStringAsync("api/genres");

        var movies = JsonConvert.DeserializeObject<List<MovieModel>>(moviesJson) ?? new();
        var series = JsonConvert.DeserializeObject<List<SeriesModel>>(seriesJson) ?? new();
        var genres = JsonConvert.DeserializeObject<List<GenreModel>>(genresJson) ?? new();

        ViewBag.Movies       = movies.Take(10).ToList();
        ViewBag.Series       = series.Take(6).ToList();
        ViewBag.Genres       = genres;

        var featuredSeries = series.FirstOrDefault(s => s.Title.Contains("Supernatural", StringComparison.OrdinalIgnoreCase));
        if (featuredSeries != null)
        {
            ViewBag.Featured = featuredSeries;
            ViewBag.FeaturedType = "Series";
        }
        else
        {
            ViewBag.Featured = movies.FirstOrDefault();
            ViewBag.FeaturedType = "Movie";
        }
        return View();
    }

    public async Task<IActionResult> Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return RedirectToAction(nameof(Index));

        var json    = await _http.GetStringAsync($"api/reports/search?q={Uri.EscapeDataString(q)}");
        var results = JsonConvert.DeserializeObject<List<SearchResultModel>>(json) ?? new();

        ViewBag.Query   = q;
        ViewBag.Results = results;
        return View();
    }

    public IActionResult Error() => View();
}
