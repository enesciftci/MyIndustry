using FluentAssertions;
using MyIndustry.ApplicationService.Helpers;

namespace MyIndustry.Tests.Unit.Helpers;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Çelik Boru Üretimi", "celik-boru-uretimi")]
    [InlineData("İstanbul Şişli", "istanbul-sisli")]
    [InlineData("Öğrenci Gözlem", "ogrenci-gozlem")]
    public void GenerateSlug_Should_Convert_Turkish_Characters(string input, string expected)
    {
        SlugHelper.GenerateSlug(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateSlug_Should_Return_Empty_For_Blank_Input(string? input)
    {
        SlugHelper.GenerateSlug(input!).Should().BeEmpty();
    }

    [Theory]
    [InlineData("Hello @World!!!", "hello-world")]
    [InlineData("  Multiple   Spaces  ", "multiple-spaces")]
    [InlineData("test---slug", "test-slug")]
    public void GenerateSlug_Should_Handle_Special_Characters(string input, string expected)
    {
        SlugHelper.GenerateSlug(input).Should().Be(expected);
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_Should_Return_Base_Slug_When_Not_Exists()
    {
        var result = await SlugHelper.GenerateUniqueSlugAsync("my-slug", _ => Task.FromResult(false));

        result.Should().Be("my-slug");
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_Should_Append_Counter_When_Slug_Exists()
    {
        var existing = new HashSet<string> { "my-slug", "my-slug-1" };

        var result = await SlugHelper.GenerateUniqueSlugAsync("my-slug", slug => Task.FromResult(existing.Contains(slug)));

        result.Should().Be("my-slug-2");
    }

    [Fact]
    public async Task GenerateUniqueSlugAsync_Should_Return_Empty_For_Blank_Base()
    {
        var result = await SlugHelper.GenerateUniqueSlugAsync("  ", _ => Task.FromResult(true));

        result.Should().BeEmpty();
    }
}
