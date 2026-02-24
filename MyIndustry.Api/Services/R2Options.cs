namespace MyIndustry.Api.Services;

public class R2Options
{
    public const string SectionName = "R2";

    /// <summary>Cloudflare hesap ID (Dashboard URL'deki).</summary>
    public string AccountId { get; set; } = "";

    /// <summary>R2 API Token ile oluşturulan Access Key ID.</summary>
    public string AccessKeyId { get; set; } = "";

    /// <summary>R2 API Token Secret Access Key.</summary>
    public string SecretAccessKey { get; set; } = "";

    /// <summary>Bucket adı.</summary>
    public string BucketName { get; set; } = "uploads";

    /// <summary>
    /// Yüklenen dosyaların public URL ön eki.
    /// R2.dev: https://pub-xxxx.r2.dev
    /// Custom domain: https://cdn.example.com veya https://uploads.example.com
    /// </summary>
    public string PublicBaseUrl { get; set; } = "";
}
