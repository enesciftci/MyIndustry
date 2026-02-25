using FluentAssertions;
using MyIndustry.ApplicationService.Helpers;

namespace MyIndustry.Tests.Unit.Helpers;

public class SearchTermHelperTests
{
    [Fact]
    public void GetSearchVariants_When_Null_Returns_Empty()
    {
        var result = SearchTermHelper.GetSearchVariants(null);
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void GetSearchVariants_When_WhiteSpace_Returns_Empty()
    {
        SearchTermHelper.GetSearchVariants("").Should().BeEmpty();
        SearchTermHelper.GetSearchVariants("   ").Should().BeEmpty();
    }

    [Fact]
    public void GetSearchVariants_Returns_Original_Lowercase()
    {
        var result = SearchTermHelper.GetSearchVariants("Kompresör");
        result.Should().Contain("kompresör");
    }

    [Fact]
    public void GetSearchVariants_Normalizes_Turkish_Characters()
    {
        var result = SearchTermHelper.GetSearchVariants("kompresör");
        result.Should().Contain("kompresor");
    }

    [Fact]
    public void GetSearchVariants_Adds_Double_S_When_Ends_With_VowelSor()
    {
        var result = SearchTermHelper.GetSearchVariants("kompresor");
        result.Should().Contain("kompressor");
    }

    [Fact]
    public void GetSearchVariants_Collapses_Double_S()
    {
        var result = SearchTermHelper.GetSearchVariants("kompressor");
        result.Should().Contain("kompresor");
    }

    [Fact]
    public void GetSearchVariants_All_Turkish_Chars_Normalized()
    {
        var result = SearchTermHelper.GetSearchVariants("çğıöşü");
        result.Should().Contain("cgiosu"); // ç->c, ğ->g, ı->i, ö->o, ş->s, ü->u
        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void GetSearchVariants_Trims_Input()
    {
        var result = SearchTermHelper.GetSearchVariants("  forklift  ");
        result.Should().Contain("forklift");
    }

    [Fact]
    public void GetSearchVariants_Kompresor_Produces_Expected_Variants()
    {
        var result = SearchTermHelper.GetSearchVariants("kompresör");
        result.Should().Contain("kompresör");
        result.Should().Contain("kompresor");
        result.Should().Contain("kompressor");
    }
}
