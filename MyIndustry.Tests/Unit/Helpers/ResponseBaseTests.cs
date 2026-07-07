using FluentAssertions;
using MyIndustry.ApplicationService.Handler;

namespace MyIndustry.Tests.Unit.Helpers;

public class ResponseBaseTests
{
    [Fact]
    public void IsTimeout_Should_Return_True_When_MessageCode_Is_1907()
    {
        var response = new ResponseBase { MessageCode = "1907" };

        response.IsTimeout().Should().BeTrue();
    }

    [Fact]
    public void IsTimeout_Should_Return_False_When_MessageCode_Is_Not_1907()
    {
        var response = new ResponseBase { MessageCode = "0000" };

        response.IsTimeout().Should().BeFalse();
    }

    [Fact]
    public void IsTimeout_Should_Return_False_When_MessageCode_Is_Null()
    {
        var response = new ResponseBase();

        response.IsTimeout().Should().BeFalse();
    }
}
