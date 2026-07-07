using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using MyIndustry.Api.Services;

namespace MyIndustry.Tests.Unit.Api;

public class LocalImageStorageServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _webRoot;

    public LocalImageStorageServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "myindustry-tests", Guid.NewGuid().ToString());
        _webRoot = Path.Combine(_tempRoot, "wwwroot");
        Directory.CreateDirectory(_webRoot);
    }

    [Fact]
    public async Task UploadAsync_Should_Save_File_And_Return_Public_Url()
    {
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns(_webRoot);
        envMock.Setup(e => e.ContentRootPath).Returns(_tempRoot);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost", 5001);
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

        var service = new LocalImageStorageService(envMock.Object, httpContextAccessor.Object);
        await using var stream = new MemoryStream("image-bytes"u8.ToArray());

        var url = await service.UploadAsync(stream, "photo.jpg", "image/jpeg");

        url.Should().StartWith("https://localhost:5001/uploads/");
        url.Should().EndWith(".jpg");

        var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
        File.Exists(Path.Combine(_webRoot, "uploads", fileName)).Should().BeTrue();
    }

    [Fact]
    public async Task UploadAsync_Should_Use_ContentRoot_When_WebRootPath_Is_Null()
    {
        var envMock = new Mock<IWebHostEnvironment>();
        envMock.Setup(e => e.WebRootPath).Returns((string?)null!);
        envMock.Setup(e => e.ContentRootPath).Returns(_tempRoot);

        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        var service = new LocalImageStorageService(envMock.Object, httpContextAccessor.Object);
        await using var stream = new MemoryStream("image-bytes"u8.ToArray());

        var url = await service.UploadAsync(stream, "photo.png", "image/png");

        url.Should().StartWith("/uploads/");
        Directory.Exists(Path.Combine(_tempRoot, "wwwroot", "uploads")).Should().BeTrue();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, true);
    }
}
