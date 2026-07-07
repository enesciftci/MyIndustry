namespace MyIndustry.Api.Services;

public interface IImageUploadValidator
{
    void Validate(IFormFile file);
}

public class ImageUploadValidator : IImageUploadValidator
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly Dictionary<string, byte[][]> MagicBytes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        [".webp"] = new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } }
    };

    private readonly long _maxBytes;

    public ImageUploadValidator(IConfiguration configuration)
    {
        _maxBytes = configuration.GetValue("ImageUpload:MaxBytes", 5L * 1024 * 1024);
    }

    public void Validate(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("Dosya boş olamaz.");

        if (file.Length > _maxBytes)
            throw new InvalidOperationException($"Dosya boyutu {_maxBytes / (1024 * 1024)} MB sınırını aşıyor.");

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Yalnızca JPG, PNG ve WebP dosyaları yüklenebilir.");

        using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = stream.Read(header, 0, header.Length);
        if (read < 3 || !HasValidMagicBytes(extension, header))
            throw new InvalidOperationException("Dosya içeriği geçerli bir görsel formatıyla eşleşmiyor.");
    }

    private static bool HasValidMagicBytes(string extension, byte[] header)
    {
        if (!MagicBytes.TryGetValue(extension, out var signatures))
            return false;

        return signatures.Any(signature =>
            header.Length >= signature.Length &&
            header.Take(signature.Length).SequenceEqual(signature));
    }
}
