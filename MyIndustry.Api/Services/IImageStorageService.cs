namespace MyIndustry.Api.Services;

/// <summary>
/// Görsel yükleme (örn. Cloudflare R2 / S3) için soyutlama.
/// </summary>
public interface IImageStorageService
{
    /// <summary>
    /// Stream'i storage'a yükler ve public URL döner.
    /// </summary>
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
}
