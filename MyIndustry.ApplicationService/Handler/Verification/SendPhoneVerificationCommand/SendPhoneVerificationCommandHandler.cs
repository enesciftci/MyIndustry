using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Queue.Message;

namespace MyIndustry.ApplicationService.Handler.Verification.SendPhoneVerificationCommand;

public sealed class SendPhoneVerificationCommandHandler : IRequestHandler<SendPhoneVerificationCommand, SendPhoneVerificationCommandResult>
{
    private readonly IGenericRepository<PhoneVerification> _phoneVerificationRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private const int CodeExpirationMinutes = 5;
    private const int MaxAttemptsPerHour = 5;

    public SendPhoneVerificationCommandHandler(
        IGenericRepository<PhoneVerification> phoneVerificationRepository,
        IPublishEndpoint publishEndpoint)
    {
        _phoneVerificationRepository = phoneVerificationRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<SendPhoneVerificationCommandResult> Handle(SendPhoneVerificationCommand request, CancellationToken cancellationToken)
    {
        // Rate limiting - son 1 saatte kaç deneme yapılmış
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentAttempts = await _phoneVerificationRepository
            .GetAllQuery()
            .CountAsync(p => p.UserId == request.UserId 
                          && p.PhoneNumber == request.PhoneNumber 
                          && p.CreatedDate > oneHourAgo, cancellationToken);

        if (recentAttempts >= MaxAttemptsPerHour)
        {
            return new SendPhoneVerificationCommandResult
            {
                Success = false,
                Message = "Çok fazla deneme yaptınız. Lütfen 1 saat sonra tekrar deneyin."
            };
        }

        // Check if there's an active verification code
        var existingCode = await _phoneVerificationRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId 
                                   && p.PhoneNumber == request.PhoneNumber 
                                   && !p.IsUsed 
                                   && p.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (existingCode != null)
        {
            var remainingSeconds = (int)(existingCode.ExpiresAt - DateTime.UtcNow).TotalSeconds;
            return new SendPhoneVerificationCommandResult
            {
                Success = false,
                Message = $"Aktif bir doğrulama kodu zaten mevcut. {remainingSeconds} saniye sonra yeni kod isteyebilirsiniz.",
                ExpiresInSeconds = remainingSeconds
            };
        }

        // Generate 6-digit code
        var code = new Random().Next(100000, 999999).ToString();

        var verification = new PhoneVerification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            PhoneNumber = request.PhoneNumber,
            VerificationCode = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(CodeExpirationMinutes),
            IsUsed = false,
            AttemptCount = 0,
            CreatedDate = DateTime.UtcNow
        };

        await _phoneVerificationRepository.AddAsync(verification, cancellationToken);

        // Send SMS via RabbitMQ
        await _publishEndpoint.Publish(new SendPhoneVerificationMessage
        {
            PhoneNumber = request.PhoneNumber,
            VerificationCode = code
        }, cancellationToken);

        return new SendPhoneVerificationCommandResult
        {
            Message = "Doğrulama kodu gönderildi",
            ExpiresInSeconds = CodeExpirationMinutes * 60
        }.ReturnOk();
    }
}
