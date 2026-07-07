using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MyIndustry.Container.Logging;

namespace MyIndustry.Tests.Unit.Logging;

public class SensitiveDataMaskerTests
{
    private readonly MediatRLoggingOptions _options = new();

    [Fact]
    public void Sanitize_RedactsPasswordProperty()
    {
        var payload = new TestRequest { Password = "super-secret" };

        var result = SensitiveDataMasker.Sanitize(payload, _options);

        result["Password"].Should().Be("***");
    }

    [Fact]
    public void Sanitize_PartiallyMasksEmail()
    {
        var payload = new TestRequest { Email = "user@example.com" };

        var result = SensitiveDataMasker.Sanitize(payload, _options);

        result["Email"].Should().Be("u***@example.com");
    }

    [Fact]
    public void Sanitize_TruncatesLongContent()
    {
        var payload = new TestRequest { Content = new string('a', 300) };

        var result = SensitiveDataMasker.Sanitize(payload, _options);

        var truncated = result["Content"] as string;
        truncated.Should().NotBeNull();
        truncated!.Length.Should().BeLessThan(300);
        truncated.Should().EndWith("...");
    }

    [Fact]
    public void Sanitize_RespectsLogSensitiveAttribute()
    {
        var payload = new TestRequestWithAttribute { CustomField = "sensitive-value" };

        var result = SensitiveDataMasker.Sanitize(payload, _options);

        result.Should().NotContainKey("IgnoredField");
        result["CustomField"].Should().Be("s***e");
    }

    [Fact]
    public void Sanitize_LimitsNestedDepth()
    {
        var payload = new TestRequest
        {
            Nested = new NestedRequest
            {
                Nested = new NestedRequest
                {
                    Nested = new NestedRequest { Name = "deep" }
                }
            }
        };

        var serialized = System.Text.Json.JsonSerializer.Serialize(
            SensitiveDataMasker.Sanitize(payload, _options));

        serialized.Should().Contain("[MaxDepthReached]");
    }

    [Fact]
    public void Sanitize_LogsIFormFileMetadataOnly()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("photo.jpg");
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        var payload = new TestRequest { Upload = fileMock.Object };

        var result = SensitiveDataMasker.Sanitize(payload, _options);
        var fileResult = result["Upload"] as Dictionary<string, object?>;

        fileResult!["fileName"].Should().Be("photo.jpg");
        fileResult["length"].Should().Be(1024L);
        fileResult["contentType"].Should().Be("image/jpeg");
    }

    private sealed class TestRequest
    {
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Content { get; set; }
        public IFormFile? Upload { get; set; }
        public NestedRequest? Nested { get; set; }
    }

    private sealed class NestedRequest
    {
        public string? Name { get; set; }
        public NestedRequest? Nested { get; set; }
    }

    private sealed class TestRequestWithAttribute
    {
        [LogSensitive(Mode = LogMaskMode.Partial)]
        public string CustomField { get; set; } = "sensitive-value";

        [LogSensitive(Mode = LogMaskMode.Ignore)]
        public string IgnoredField { get; set; } = "hidden";
    }
}
