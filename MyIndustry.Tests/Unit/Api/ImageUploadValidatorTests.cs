using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using MyIndustry.Api.Services;

namespace MyIndustry.Tests.Unit.Api;

public class ImageUploadValidatorTests
{
    private readonly ImageUploadValidator _validator = new(new ConfigurationBuilder().Build());

    [Fact]
    public void Validate_Should_Accept_Valid_Png()
    {
        var file = CreateFormFile("test.png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

        var act = () => _validator.Validate(file);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_Should_Reject_Invalid_Extension()
    {
        var file = CreateFormFile("test.php", new byte[] { 0x89, 0x50, 0x4E, 0x47 });

        var act = () => _validator.Validate(file);

        act.Should().Throw<InvalidOperationException>().WithMessage("*JPG, PNG ve WebP*");
    }

    [Fact]
    public void Validate_Should_Reject_Invalid_Magic_Bytes()
    {
        var file = CreateFormFile("test.png", new byte[] { 0x00, 0x00, 0x00, 0x00 });

        var act = () => _validator.Validate(file);

        act.Should().Throw<InvalidOperationException>().WithMessage("*geçerli bir görsel*");
    }

    private static IFormFile CreateFormFile(string fileName, byte[] content)
    {
        var stream = new MemoryStream(content);
        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.Length).Returns(content.Length);
        file.Setup(f => f.OpenReadStream()).Returns(stream);
        return file.Object;
    }
}
