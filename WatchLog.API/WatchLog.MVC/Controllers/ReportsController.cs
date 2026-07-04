using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WatchLog.MVC.Data;
using WatchLog.MVC.Models.ApiModels;
using WatchLog.MVC.Models.ViewModels;

namespace WatchLog.MVC.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly HttpClient   _http;
    private readonly AppDbContext _db;

    public ReportsController(IHttpClientFactory factory, AppDbContext db)
    {
        _http = factory.CreateClient("WatchLogAPI");
        _db   = db;
    }

    // GET /Reports
    public async Task<IActionResult> Index()
    {
        var genresJson = await _http.GetStringAsync("api/genres");
        var actorsJson = await _http.GetStringAsync("api/actors");
        ViewBag.Genres = JsonConvert.DeserializeObject<List<GenreModel>>(genresJson) ?? new();
        ViewBag.Actors = JsonConvert.DeserializeObject<List<ActorModel>>(actorsJson) ?? new();
        return View();
    }

    // GET /Reports/Movies
    public async Task<IActionResult> Movies(int? genreId)
    {
        var url    = genreId.HasValue ? $"api/movies/by-genre/{genreId}" : "api/movies";
        var json   = await _http.GetStringAsync(url);
        var movies = JsonConvert.DeserializeObject<List<MovieModel>>(json) ?? new();

        var genresJson = await _http.GetStringAsync("api/genres");
        ViewBag.Genres        = JsonConvert.DeserializeObject<List<GenreModel>>(genresJson) ?? new();
        ViewBag.SelectedGenre = genreId;
        return View(movies);
    }

    // GET /Reports/Series
    public async Task<IActionResult> Series(int? genreId)
    {
        var url    = genreId.HasValue ? $"api/series/by-genre/{genreId}" : "api/series";
        var json   = await _http.GetStringAsync(url);
        var series = JsonConvert.DeserializeObject<List<SeriesModel>>(json) ?? new();

        var genresJson = await _http.GetStringAsync("api/genres");
        ViewBag.Genres        = JsonConvert.DeserializeObject<List<GenreModel>>(genresJson) ?? new();
        ViewBag.SelectedGenre = genreId;
        return View(series);
    }

    // GET /Reports/GenreStats
    public async Task<IActionResult> GenreStats()
    {
        var json  = await _http.GetStringAsync("api/genres/stats");
        var stats = JsonConvert.DeserializeObject<List<GenreStatsModel>>(json) ?? new();
        return View(stats);
    }

    // GET /Reports/UserActivity
    public async Task<IActionResult> UserActivity()
    {
        ViewBag.ReviewCount   = await _db.Reviews.CountAsync(r => r.IsActive);
        ViewBag.RatingCount   = await _db.Ratings.CountAsync(r => r.IsActive);
        ViewBag.FavoriteCount = await _db.Favorites.CountAsync(f => f.IsActive);

        var topRatedGroup = await _db.Ratings
            .Where(r => r.IsActive)
            .GroupBy(r => new { r.ContentId, r.ContentType })
            .Select(g => new { g.Key.ContentId, g.Key.ContentType, Avg = g.Average(r => r.Score), Count = g.Count() })
            .OrderByDescending(x => x.Avg)
            .Take(5)
            .ToListAsync();

        var moviesJson = await _http.GetStringAsync("api/movies");
        var seriesJson = await _http.GetStringAsync("api/series");
        var movies = JsonConvert.DeserializeObject<List<MovieModel>>(moviesJson) ?? new();
        var series = JsonConvert.DeserializeObject<List<SeriesModel>>(seriesJson) ?? new();

        var topRated = topRatedGroup.Select(x =>
        {
            var title = "Bilinmiyor";
            if (x.ContentType == "Movie")
            {
                title = movies.FirstOrDefault(m => m.Id == x.ContentId)?.Title ?? "Bilinmiyor";
            }
            else
            {
                title = series.FirstOrDefault(s => s.Id == x.ContentId)?.Title ?? "Bilinmiyor";
            }

            return new TopRatedContentViewModel
            {
                ContentId = x.ContentId,
                ContentType = x.ContentType,
                Title = title,
                AverageScore = Math.Round(x.Avg, 1),
                RatingCount = x.Count
            };
        }).ToList();

        ViewBag.TopRated = topRated;
        return View();
    }

    // ── Excel Export ────────────────────────────────────────────────────────

    // GET /Reports/ExportMoviesExcel
    public async Task<IActionResult> ExportMoviesExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var json   = await _http.GetStringAsync("api/reports/movies-details");
        var movies = JsonConvert.DeserializeObject<List<MovieReportModel>>(json) ?? new();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Filmler");

        string[] headers = { "ID", "Başlık", "Yıl", "Süre (dk)", "Yönetmen", "Ülke", "Dil", "Türler", "Oyuncular" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#D92243"));
            ws.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
            ws.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        int row = 2;
        foreach (var m in movies)
        {
            ws.Cells[row, 1].Value = m.Id;
            ws.Cells[row, 2].Value = m.Title;
            ws.Cells[row, 3].Value = m.ReleaseYear;
            ws.Cells[row, 4].Value = m.Duration;
            ws.Cells[row, 5].Value = m.Director;
            ws.Cells[row, 6].Value = m.Country;
            ws.Cells[row, 7].Value = m.Language;
            ws.Cells[row, 8].Value = m.Genres;
            ws.Cells[row, 9].Value = m.Actors;
            if (row % 2 == 0)
            {
                ws.Cells[row, 1, row, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, 1, row, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#F9F9F9"));
            }
            row++;
        }
        ws.Cells.AutoFitColumns();

        var bytes = package.GetAsByteArray();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"WatchLog_Filmler_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // GET /Reports/ExportSeriesExcel
    public async Task<IActionResult> ExportSeriesExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var json       = await _http.GetStringAsync("api/reports/series-details");
        var seriesList = JsonConvert.DeserializeObject<List<SeriesReportModel>>(json) ?? new();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Diziler");

        string[] headers = { "ID", "Başlık", "Başlangıç", "Bitiş", "Sezon", "Bölüm", "Yönetmen", "Ülke", "Dil", "Türler" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#D92243"));
            ws.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        int row = 2;
        foreach (var s in seriesList)
        {
            ws.Cells[row, 1].Value  = s.Id;
            ws.Cells[row, 2].Value  = s.Title;
            ws.Cells[row, 3].Value  = s.StartYear;
            ws.Cells[row, 4].Value  = s.EndYear?.ToString() ?? "Devam Ediyor";
            ws.Cells[row, 5].Value  = s.SeasonCount;
            ws.Cells[row, 6].Value  = s.EpisodeCount;
            ws.Cells[row, 7].Value  = s.Director;
            ws.Cells[row, 8].Value  = s.Country;
            ws.Cells[row, 9].Value  = s.Language;
            ws.Cells[row, 10].Value = s.Genres;
            if (row % 2 == 0)
            {
                ws.Cells[row, 1, row, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, 1, row, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#F9F9F9"));
            }
            row++;
        }
        ws.Cells.AutoFitColumns();

        var bytes = package.GetAsByteArray();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"WatchLog_Diziler_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    // ── PDF Export ──────────────────────────────────────────────────────────

    // GET /Reports/ExportMoviesPdf
    public async Task<IActionResult> ExportMoviesPdf()
    {
        var json   = await _http.GetStringAsync("api/reports/movies-details");
        var movies = JsonConvert.DeserializeObject<List<MovieReportModel>>(json) ?? new();

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Text("WatchLog")
                        .FontSize(22).SemiBold().FontColor(Color.FromHex("D92243"));
                    col.Item().Text("Film Raporu — " + DateTime.Now.ToString("dd MMMM yyyy"))
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Color.FromHex("D92243"));
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(25);
                        c.RelativeColumn(3);
                        c.ConstantColumn(35);
                        c.ConstantColumn(45);
                        c.RelativeColumn(2);
                        c.RelativeColumn(1.5f);
                        c.RelativeColumn(2);
                    });

                    table.Header(h =>
                    {
                        foreach (var col in new[] { "ID", "Başlık", "Yıl", "Süre", "Yönetmen", "Ülke", "Türler" })
                            h.Cell().Background(Color.FromHex("D92243")).Padding(5)
                             .Text(col).FontColor(Colors.White).SemiBold().FontSize(9);
                    });

                    bool alt = false;
                    foreach (var m in movies)
                    {
                        var bg = alt ? Color.FromHex("F5F5F5") : Colors.White;
                        alt = !alt;
                        table.Cell().Background(bg).Padding(4).Text(m.Id.ToString());
                        table.Cell().Background(bg).Padding(4).Text(m.Title).SemiBold();
                        table.Cell().Background(bg).Padding(4).Text(m.ReleaseYear?.ToString() ?? "-");
                        table.Cell().Background(bg).Padding(4).Text(m.Duration != null ? $"{m.Duration} dk" : "-");
                        table.Cell().Background(bg).Padding(4).Text(m.Director ?? "-");
                        table.Cell().Background(bg).Padding(4).Text(m.Country ?? "-");
                        table.Cell().Background(bg).Padding(4).Text(m.Genres ?? "-");
                    }
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("WatchLog © ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(DateTime.Now.Year.ToString()).FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" | Sayfa ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf();

        return File(bytes, "application/pdf", $"WatchLog_Filmler_{DateTime.Now:yyyyMMdd}.pdf");
    }

    // GET /Reports/ExportSeriesPdf
    public async Task<IActionResult> ExportSeriesPdf()
    {
        var json       = await _http.GetStringAsync("api/reports/series-details");
        var seriesList = JsonConvert.DeserializeObject<List<SeriesReportModel>>(json) ?? new();

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Text("WatchLog")
                        .FontSize(22).SemiBold().FontColor(Color.FromHex("D92243"));
                    col.Item().Text("Dizi Raporu — " + DateTime.Now.ToString("dd MMMM yyyy"))
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Color.FromHex("D92243"));
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(25);
                        c.RelativeColumn(3);
                        c.ConstantColumn(55);
                        c.ConstantColumn(55);
                        c.ConstantColumn(40);
                        c.ConstantColumn(40);
                        c.RelativeColumn(1.5f);
                        c.RelativeColumn(2);
                    });

                    table.Header(h =>
                    {
                        foreach (var col in new[] { "ID", "Başlık", "Başlangıç", "Bitiş", "Sezon", "Bölüm", "Ülke", "Türler" })
                            h.Cell().Background(Color.FromHex("D92243")).Padding(5)
                             .Text(col).FontColor(Colors.White).SemiBold().FontSize(9);
                    });

                    bool alt = false;
                    foreach (var s in seriesList)
                    {
                        var bg = alt ? Color.FromHex("F5F5F5") : Colors.White;
                        alt = !alt;
                        table.Cell().Background(bg).Padding(4).Text(s.Id.ToString());
                        table.Cell().Background(bg).Padding(4).Text(s.Title).SemiBold();
                        table.Cell().Background(bg).Padding(4).Text(s.StartYear?.ToString() ?? "-");
                        table.Cell().Background(bg).Padding(4).Text(s.EndYear?.ToString() ?? "Devam Ediyor");
                        table.Cell().Background(bg).Padding(4).Text(s.SeasonCount?.ToString() ?? "-");
                        table.Cell().Background(bg).Padding(4).Text(s.EpisodeCount?.ToString() ?? "-");
                        table.Cell().Background(bg).Padding(4).Text(s.Country ?? "-");
                        table.Cell().Background(bg).Padding(4).Text(s.Genres ?? "-");
                    }
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("WatchLog © ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(DateTime.Now.Year.ToString()).FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" | Sayfa ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                    x.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        }).GeneratePdf();

        return File(bytes, "application/pdf", $"WatchLog_Diziler_{DateTime.Now:yyyyMMdd}.pdf");
    }
}
