using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;

namespace MyIndustry.Api.Services;

/// <summary>
/// Cloudflare R2 (S3 uyumlu) bucket'a görsel yükler.
/// R2 Dashboard: API Tokens ile Access Key ID ve Secret Access Key alınır.
/// Public erişim: Bucket için R2.dev subdomain veya custom domain tanımlanır; PublicBaseUrl bu adres olmalı.
/// </summary>
public class R2ImageStorageService : IImageStorageService
{
    private readonly AmazonS3Client _client;
    private readonly string _bucketName;
    private readonly string _publicBaseUrl;

    public R2ImageStorageService(R2Options options)
    {
        if (string.IsNullOrEmpty(options.AccountId) || string.IsNullOrEmpty(options.AccessKeyId) || string.IsNullOrEmpty(options.SecretAccessKey))
            throw new InvalidOperationException("R2:AccountId, AccessKeyId ve SecretAccessKey appsettings'te tanımlı olmalı.");

        _bucketName = options.BucketName ?? "uploads";
        _publicBaseUrl = (options.PublicBaseUrl ?? "").TrimEnd('/');

        var endpoint = $"https://{options.AccountId}.r2.cloudflarestorage.com";
        var credentials = new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey);

        _client = new AmazonS3Client(credentials, new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
            SignatureVersion = "v4",
        });
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var key = $"uploads/{Guid.NewGuid()}{Path.GetExtension(fileName)}";

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            DisablePayloadSigning = true, // R2 requirement
        };

        await _client.PutObjectAsync(request, cancellationToken);

        if (string.IsNullOrEmpty(_publicBaseUrl))
            throw new InvalidOperationException("R2:PublicBaseUrl appsettings'te tanımlı olmalı (örn. https://pub-xxx.r2.dev veya custom domain).");

        return $"{_publicBaseUrl}/{key}";
    }
}
