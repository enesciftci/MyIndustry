using System.Reflection;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using MyIndustry.Api.Services;

namespace MyIndustry.Tests.Unit.Api;

public class R2ImageStorageServiceTests
{
    [Fact]
    public void Constructor_Should_Throw_When_Options_Incomplete()
    {
        var options = new R2Options { AccountId = "", AccessKeyId = "", SecretAccessKey = "" };

        var act = () => new R2ImageStorageService(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*AccountId, AccessKeyId ve SecretAccessKey*");
    }

    [Fact]
    public async Task UploadAsync_Should_Return_Public_Url_When_S3_Succeeds()
    {
        var options = new R2Options
        {
            AccountId = "test-account",
            AccessKeyId = "test-key",
            SecretAccessKey = "test-secret",
            BucketName = "uploads",
            PublicBaseUrl = "https://cdn.example.com"
        };

        var service = new R2ImageStorageService(options);
        var fakeClient = new FakeAmazonS3Client();
        SetClient(service, fakeClient);

        await using var stream = new MemoryStream("image-bytes"u8.ToArray());
        var url = await service.UploadAsync(stream, "photo.jpg", "image/jpeg");

        url.Should().StartWith("https://cdn.example.com/uploads/");
        url.Should().EndWith(".jpg");
        fakeClient.PutObjectCalled.Should().BeTrue();
        fakeClient.LastRequest.Should().NotBeNull();
        fakeClient.LastRequest!.BucketName.Should().Be("uploads");
        fakeClient.LastRequest.ContentType.Should().Be("image/jpeg");
        fakeClient.LastRequest.DisablePayloadSigning.Should().BeTrue();
    }

    [Fact]
    public async Task UploadAsync_Should_Throw_When_PublicBaseUrl_Missing()
    {
        var options = new R2Options
        {
            AccountId = "test-account",
            AccessKeyId = "test-key",
            SecretAccessKey = "test-secret",
            BucketName = "uploads",
            PublicBaseUrl = ""
        };

        var service = new R2ImageStorageService(options);
        SetClient(service, new FakeAmazonS3Client());

        await using var stream = new MemoryStream("image-bytes"u8.ToArray());
        var act = () => service.UploadAsync(stream, "photo.jpg", "image/jpeg");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*PublicBaseUrl*");
    }

    private static void SetClient(R2ImageStorageService service, AmazonS3Client client)
    {
        typeof(R2ImageStorageService)
            .GetField("_client", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(service, client);
    }

    private sealed class FakeAmazonS3Client : AmazonS3Client
    {
        public bool PutObjectCalled { get; private set; }
        public PutObjectRequest? LastRequest { get; private set; }

        public FakeAmazonS3Client()
            : base(new BasicAWSCredentials("test", "test"), new AmazonS3Config
            {
                ServiceURL = "http://localhost",
                ForcePathStyle = true
            })
        {
        }

        public override Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request, CancellationToken cancellationToken = default)
        {
            PutObjectCalled = true;
            LastRequest = request;
            return Task.FromResult(new PutObjectResponse { HttpStatusCode = System.Net.HttpStatusCode.OK });
        }
    }
}
