using FluentAssertions;
using MyIndustry.ApplicationService.Handler;

namespace MyIndustry.Tests.Unit.Helpers;

public class ResponseBaseExtensionsTests
{
    private sealed record TestResponse : ResponseBase;

    [Fact]
    public void ReturnOk_Should_Set_Success_And_Default_Messages()
    {
        var response = new TestResponse().ReturnOk();

        response.Success.Should().BeTrue();
        response.MessageCode.Should().Be("0000");
        response.Message.Should().Be("İşlem başarıyla gerçekleştirildi.");
        response.UserMessage.Should().Be("İşlem başarıyla gerçekleştirildi.");
    }

    [Fact]
    public void ReturnOk_With_Message_Should_Set_Custom_Message()
    {
        var response = new TestResponse().ReturnOk("Custom success");

        response.Success.Should().BeTrue();
        response.MessageCode.Should().Be("0000");
        response.Message.Should().Be("Custom success");
        response.UserMessage.Should().Be("Custom success");
    }

    [Fact]
    public void ReturnBad_Should_Set_Failure_And_Default_Messages()
    {
        var response = new TestResponse().ReturnBad();

        response.Success.Should().BeFalse();
        response.MessageCode.Should().Be("1001");
        response.Message.Should().Be("Bir hata oluştu");
        response.UserMessage.Should().Be("Bir hata oluştu");
    }

    [Fact]
    public void ReturnBadRequest_Should_Set_Custom_Error_Message()
    {
        var response = new TestResponse().ReturnBadRequest("Invalid input");

        response.Success.Should().BeFalse();
        response.MessageCode.Should().Be("1001");
        response.Message.Should().Be("Invalid input");
        response.UserMessage.Should().Be("Invalid input");
    }

    [Fact]
    public void ReturnNotFound_Should_Set_NotFound_Code()
    {
        var response = new TestResponse().ReturnNotFound("Not found");

        response.Success.Should().BeFalse();
        response.MessageCode.Should().Be("1004");
        response.Message.Should().Be("Not found");
        response.UserMessage.Should().Be("Not found");
    }

    [Fact]
    public void Return_Should_Set_All_Properties()
    {
        var logParams = new Dictionary<string, string> { ["key"] = "value" };
        var response = new TestResponse().Return(false, "9999", "System error", "User friendly", logParams);

        response.Success.Should().BeFalse();
        response.MessageCode.Should().Be("9999");
        response.Message.Should().Be("System error");
        response.UserMessage.Should().Be("User friendly");
    }
}
