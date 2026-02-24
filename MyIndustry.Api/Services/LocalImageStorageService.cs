namespace MyIndustry.Api.Services;

/// <summary>
/// Görselleri sunucu diskine (wwwroot/uploads) kaydeder. R2 yapılandırılmadığında kullanılır.
/// </summary>
public class LocalImageStorageService : IImageStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalImageStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uploadsPath = _env.WebRootPath != null
            ? Path.Combine(_env.WebRootPath, "uploads")
            : Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");

        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var safeFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(uploadsPath, safeFileName);

        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = request != null ? $"{request.Scheme}://{request.Host}" : "";
        return $"{baseUrl}/uploads/{safeFileName}";
    }
}
