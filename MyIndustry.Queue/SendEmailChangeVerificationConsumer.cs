using CoreApiCommunicator.Email;
using MassTransit;
using MyIndustry.Queue.Message;

namespace MyIndustry.Queue;

public class SendEmailChangeVerificationConsumer : IConsumer<SendEmailChangeVerificationMessage>
{
    private readonly IEmailSender _emailSender;

    public SendEmailChangeVerificationConsumer(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task Consume(ConsumeContext<SendEmailChangeVerificationMessage> context)
    {
        var message = context.Message;
        Console.WriteLine($"Sending email change verification to: {message.Email}");

        var subject = "MyIndustry - E-posta Değişikliği Doğrulama";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>E-posta Adresinizi Doğrulayın</h2>
                <p>E-posta adresinizi değiştirmek için aşağıdaki doğrulama kodunu kullanın:</p>
                <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; text-align: center; margin: 20px 0;'>
                    <h1 style='color: #333; letter-spacing: 5px; margin: 0;'>{message.VerificationCode}</h1>
                </div>
                <p>Bu kod 15 dakika içinde geçerliliğini yitirecektir.</p>
                <p>Eğer bu işlemi siz yapmadıysanız, bu e-postayı görmezden gelebilirsiniz.</p>
                <hr style='margin-top: 30px;' />
                <p style='color: #888; font-size: 12px;'>MyIndustry - Endüstriyel Hizmetler Platformu</p>
            </body>
            </html>";

        await _emailSender.SendEmailAsync(message.Email, subject, body);
        Console.WriteLine($"Email change verification sent to: {message.Email}");
    }
}
