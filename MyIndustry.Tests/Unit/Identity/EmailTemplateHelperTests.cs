using FluentAssertions;
using MyIndustry.Identity.Domain.Service;

namespace MyIndustry.Tests.Unit.Identity;

public class EmailTemplateHelperTests
{
    [Fact]
    public void GetEmailConfirmationTemplate_Should_Return_Html_With_Confirmation_Link()
    {
        const string link = "http://localhost:3000/email-verification?userId=123&token=abc";

        var html = EmailTemplateHelper.GetEmailConfirmationTemplate("Ali", link);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("Merhaba Ali");
        html.Should().Contain(link);
        html.Should().Contain("Hesabımı Doğrula");
        html.Should().Contain("MyIndustry");
    }

    [Fact]
    public void GetPasswordResetTemplate_Should_Return_Html_With_Reset_Link()
    {
        const string link = "http://localhost:3000/reset-password?userId=123&token=xyz";

        var html = EmailTemplateHelper.GetPasswordResetTemplate("Ayşe", link);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("Merhaba Ayşe");
        html.Should().Contain(link);
        html.Should().Contain("Şifremi Sıfırla");
        html.Should().Contain("Şifre Sıfırlama");
    }

    [Fact]
    public void GetEmailChangeVerificationTemplate_Should_Return_Html_With_Verification_Code()
    {
        const string code = "123456";

        var html = EmailTemplateHelper.GetEmailChangeVerificationTemplate(null, code);

        html.Should().Contain("<!DOCTYPE html>");
        html.Should().Contain("Merhaba,");
        html.Should().Contain(code);
        html.Should().Contain("Doğrulama Kodunuz");
        html.Should().Contain("Email Değişikliği");
    }
}
