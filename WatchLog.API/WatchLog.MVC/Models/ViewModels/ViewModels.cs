using System.ComponentModel.DataAnnotations;

namespace WatchLog.MVC.Models.ViewModels;

// ── Hesap ───────────────────────────────────────────────────
public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta gereklidir.")]
    [EmailAddress]
    public string Email    { get; set; } = null!;
    [Required(ErrorMessage = "Şifre gereklidir.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Görünen ad gereklidir.")]
    [MaxLength(100)]
    public string DisplayName { get; set; } = null!;
    [Required(ErrorMessage = "E-posta gereklidir.")]
    [EmailAddress]
    public string Email       { get; set; } = null!;
    [Required(ErrorMessage = "Şifre gereklidir.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    public string Password    { get; set; } = null!;
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = null!;
}

public class ProfileViewModel
{
    public string  UserId         { get; set; } = null!;
    public string? DisplayName    { get; set; }
    public string? Email          { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public string? Bio            { get; set; }
    public int     WatchlistCount { get; set; }
    public int     FavoriteCount  { get; set; }
    public int     ReviewCount    { get; set; }
    public int     RatingCount    { get; set; }
}

// ── İzleme Listesi ──────────────────────────────────────────
public class CreateWatchlistViewModel
{
    [Required(ErrorMessage = "Liste adı gereklidir.")]
    [MaxLength(200)]
    public string  Name        { get; set; } = null!;
    public string? Description { get; set; }
    public bool    IsPublic    { get; set; }
}

public class WatchlistDetailViewModel
{
    public int    Id          { get; set; }
    public string UserId      { get; set; } = null!;
    public string Name        { get; set; } = null!;
    public string? Description { get; set; }
    public bool   IsPublic    { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WatchlistItemViewModel> Items { get; set; } = new();
}

public class WatchlistItemViewModel
{
    public int    Id          { get; set; }
    public int    ContentId   { get; set; }
    public string ContentType { get; set; } = null!;
    public string Title       { get; set; } = null!;
    public string? PosterUrl  { get; set; }
    public int?   Year        { get; set; }
    public DateTime AddedAt   { get; set; }
}

// ── Yorum / Puan ─────────────────────────────────────────────
public class AddReviewViewModel
{
    public int    ContentId   { get; set; }
    public string ContentType { get; set; } = null!;
    [Required(ErrorMessage = "Yorum metni gereklidir.")]
    [MaxLength(2000)]
    public string Text        { get; set; } = null!;
}

public class ReviewViewModel
{
    public int      Id          { get; set; }
    public string   UserName    { get; set; } = null!;
    public string   Text        { get; set; } = null!;
    public DateTime CreatedAt   { get; set; }
    public bool     IsOwner     { get; set; }
}

// ── Rapor ────────────────────────────────────────────────────
public class ReportFilterViewModel
{
    public int?    GenreId    { get; set; }
    public int?    ActorId    { get; set; }
    public string? SearchTerm { get; set; }
    public string  ReportType { get; set; } = "movies"; // "movies" | "series"
}

// ── Admin ─────────────────────────────────────────────────────
public class AdminUserViewModel
{
    public string  Id          { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? Email       { get; set; }
    public bool    IsActive    { get; set; }
    public string  Role        { get; set; } = null!;
    public DateTime CreatedAt  { get; set; }
}

public class AdminDashboardViewModel
{
    public int MovieCount    { get; set; }
    public int SeriesCount   { get; set; }
    public int ActorCount    { get; set; }
    public int GenreCount    { get; set; }
    public int UserCount     { get; set; }
    public int ReviewCount   { get; set; }
    public int FavoriteCount { get; set; }
}

public class FavoriteViewModel
{
    public int Id { get; set; }
    public int ContentId { get; set; }
    public string ContentType { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? PosterUrl { get; set; }
    public int? Year { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TopRatedContentViewModel
{
    public int ContentId { get; set; }
    public string ContentType { get; set; } = null!;
    public string Title { get; set; } = null!;
    public double AverageScore { get; set; }
    public int RatingCount { get; set; }
}

