using OfficeOpenXml;
using OfficeOpenXml.Style;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Data;
using WatchLog.API.Models.DTOs;

namespace WatchLog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IDbConnection _db;
    public ReportsController(IDbConnection db) => _db = db;

    // GET api/reports/search?q=inception
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Arama terimi boş olamaz.");

        var results = await _db.QueryAsync<SearchResultDto>(
            "sp_SearchContent", new { Query = q }, commandType: CommandType.StoredProcedure);
        return Ok(results);
    }

    // GET api/reports/movies-by-genre/1
    [HttpGet("movies-by-genre/{genreId}")]
    public async Task<IActionResult> MoviesByGenre(int genreId)
    {
        var results = await _db.QueryAsync<dynamic>(
            "sp_GetMoviesByGenre", new { GenreId = genreId }, commandType: CommandType.StoredProcedure);
        return Ok(results);
    }

    // GET api/reports/series-by-genre/1
    [HttpGet("series-by-genre/{genreId}")]
    public async Task<IActionResult> SeriesByGenre(int genreId)
    {
        var results = await _db.QueryAsync<dynamic>(
            "sp_GetSeriesByGenre", new { GenreId = genreId }, commandType: CommandType.StoredProcedure);
        return Ok(results);
    }

    // GET api/reports/movies-by-actor/1
    [HttpGet("movies-by-actor/{actorId}")]
    public async Task<IActionResult> MoviesByActor(int actorId)
    {
        var results = await _db.QueryAsync<dynamic>(
            "sp_GetMoviesByActor", new { ActorId = actorId }, commandType: CommandType.StoredProcedure);
        return Ok(results);
    }

    // GET api/reports/genre-stats
    [HttpGet("genre-stats")]
    public async Task<IActionResult> GenreStats()
    {
        var results = await _db.QueryAsync<GenreStatsDto>(
            "sp_GetGenreStats", commandType: CommandType.StoredProcedure);
        return Ok(results);
    }

    // GET api/reports/movies-details
    [HttpGet("movies-details")]
    public async Task<IActionResult> MoviesDetails()
    {
        var results = await _db.QueryAsync<MovieReportDto>(
            "sp_GetMoviesWithDetails", commandType: CommandType.StoredProcedure);
        return Ok(results);
    }

    // GET api/reports/series-details
    [HttpGet("series-details")]
    public async Task<IActionResult> SeriesDetails()
    {
        var results = await _db.QueryAsync<SeriesReportDto>(
            "sp_GetSeriesWithDetails", commandType: CommandType.StoredProcedure);
        return Ok(results);
    }

    // GET api/reports/export/movies/excel
    [HttpGet("export/movies/excel")]
    public async Task<IActionResult> ExportMoviesExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var movies = (await _db.QueryAsync<MovieReportDto>(
            "sp_GetMoviesWithDetails", commandType: CommandType.StoredProcedure)).ToList();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Filmler");

        // Başlıklar
        string[] headers = { "ID", "Başlık", "Yıl", "Süre (dk)", "Yönetmen", "Ülke", "Dil", "Türler", "Oyuncular" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#D92243"));
            ws.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        // Veriler
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
                ws.Cells[row, 1, row, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1a1a1a"));
            }
            row++;
        }

        ws.Cells.AutoFitColumns();

        var bytes = package.GetAsByteArray();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WatchLog_Filmler.xlsx");
    }

    // GET api/reports/export/series/excel
    [HttpGet("export/series/excel")]
    public async Task<IActionResult> ExportSeriesExcel()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var series = (await _db.QueryAsync<SeriesReportDto>(
            "sp_GetSeriesWithDetails", commandType: CommandType.StoredProcedure)).ToList();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Diziler");

        string[] headers = { "ID", "Başlık", "Başlangıç", "Bitiş", "Sezon", "Bölüm", "Yönetmen", "Ülke", "Dil", "Türler", "Oyuncular" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cells[1, i + 1].Value = headers[i];
            ws.Cells[1, i + 1].Style.Font.Bold = true;
            ws.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#D92243"));
            ws.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
        }

        int row = 2;
        foreach (var s in series)
        {
            ws.Cells[row, 1].Value = s.Id;
            ws.Cells[row, 2].Value = s.Title;
            ws.Cells[row, 3].Value = s.StartYear;
            ws.Cells[row, 4].Value = s.EndYear?.ToString() ?? "Devam Ediyor";
            ws.Cells[row, 5].Value = s.SeasonCount;
            ws.Cells[row, 6].Value = s.EpisodeCount;
            ws.Cells[row, 7].Value = s.Director;
            ws.Cells[row, 8].Value = s.Country;
            ws.Cells[row, 9].Value = s.Language;
            ws.Cells[row, 10].Value = s.Genres;
            ws.Cells[row, 11].Value = s.Actors;
            if (row % 2 == 0)
            {
                ws.Cells[row, 1, row, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, 1, row, 11].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#1a1a1a"));
            }
            row++;
        }

        ws.Cells.AutoFitColumns();

        var bytes = package.GetAsByteArray();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "WatchLog_Diziler.xlsx");
    }

    // GET api/reports/export/movies/pdf
    [HttpGet("export/movies/pdf")]
    public async Task<IActionResult> ExportMoviesPdf()
    {
        var movies = (await _db.QueryAsync<MovieReportDto>(
            "sp_GetMoviesWithDetails", commandType: CommandType.StoredProcedure)).ToList();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Text("WatchLog — Film Raporu")
                    .SemiBold().FontSize(16).FontColor(Color.FromHex("D92243"));

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(30);
                        c.RelativeColumn(3);
                        c.ConstantColumn(40);
                        c.ConstantColumn(50);
                        c.RelativeColumn(2);
                        c.RelativeColumn(2);
                        c.RelativeColumn(3);
                    });

                    // Başlık satırı
                    static IContainer HeaderCell(IContainer c) =>
                        c.Background(Color.FromHex("D92243")).Padding(4)
                         .AlignCenter().AlignMiddle();

                    table.Header(h =>
                    {
                        foreach (var col in new[] { "ID", "Başlık", "Yıl", "Süre", "Yönetmen", "Ülke", "Türler" })
                            h.Cell().Element(HeaderCell).Text(col).FontColor(Colors.White).SemiBold();
                    });

                    bool alt = false;
                    foreach (var m in movies)
                    {
                        var bg = alt ? Color.FromHex("1a1a1a") : Color.FromHex("141414");
                        alt = !alt;

                        static IContainer DataCell(IContainer c, Color bg) =>
                            c.Background(bg).Padding(3).AlignMiddle();

                        table.Cell().Element(c => DataCell(c, bg)).Text(m.Id.ToString()).FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(m.Title).FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(m.ReleaseYear?.ToString() ?? "-").FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(m.Duration != null ? $"{m.Duration} dk" : "-").FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(m.Director ?? "-").FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(m.Country ?? "-").FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(m.Genres ?? "-").FontColor(Colors.White);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("WatchLog | Oluşturulma: ").FontColor(Colors.Grey.Medium);
                    x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontColor(Colors.Grey.Medium);
                });
            });
        });

        var bytes = pdf.GeneratePdf();
        return File(bytes, "application/pdf", "WatchLog_Filmler.pdf");
    }

    // GET api/reports/export/series/pdf
    [HttpGet("export/series/pdf")]
    public async Task<IActionResult> ExportSeriesPdf()
    {
        var seriesList = (await _db.QueryAsync<SeriesReportDto>(
            "sp_GetSeriesWithDetails", commandType: CommandType.StoredProcedure)).ToList();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Text("WatchLog — Dizi Raporu")
                    .SemiBold().FontSize(16).FontColor(Color.FromHex("D92243"));

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(30);
                        c.RelativeColumn(3);
                        c.ConstantColumn(50);
                        c.ConstantColumn(50);
                        c.ConstantColumn(40);
                        c.ConstantColumn(40);
                        c.RelativeColumn(2);
                        c.RelativeColumn(3);
                    });

                    static IContainer HeaderCell(IContainer c) =>
                        c.Background(Color.FromHex("D92243")).Padding(4).AlignCenter().AlignMiddle();

                    table.Header(h =>
                    {
                        foreach (var col in new[] { "ID", "Başlık", "Başlangıç", "Bitiş", "Sezon", "Bölüm", "Ülke", "Türler" })
                            h.Cell().Element(HeaderCell).Text(col).FontColor(Colors.White).SemiBold();
                    });

                    bool alt = false;
                    foreach (var s in seriesList)
                    {
                        var bg = alt ? Color.FromHex("1a1a1a") : Color.FromHex("141414");
                        alt = !alt;

                        static IContainer DataCell(IContainer c, Color bg) =>
                            c.Background(bg).Padding(3).AlignMiddle();

                        table.Cell().Element(c => DataCell(c, bg)).Text(s.Id.ToString()).FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(s.Title).FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(s.StartYear?.ToString() ?? "-").FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(s.EndYear?.ToString() ?? "Devam Ediyor").FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(s.SeasonCount?.ToString() ?? "-").FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(s.EpisodeCount?.ToString() ?? "-").FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(s.Country ?? "-").FontColor(Colors.White);
                        table.Cell().Element(c => DataCell(c, bg)).Text(s.Genres ?? "-").FontColor(Colors.White);
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("WatchLog | Oluşturulma: ").FontColor(Colors.Grey.Medium);
                    x.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontColor(Colors.Grey.Medium);
                });
            });
        });

        var bytes = pdf.GeneratePdf();
        return File(bytes, "application/pdf", "WatchLog_Diziler.pdf");
    }
}
