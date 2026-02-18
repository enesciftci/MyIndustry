using MassTransit;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Queue.Message;

namespace MyIndustry.ApplicationService.Handler.Verification.SendEmailChangeVerificationCommand;

public sealed class SendEmailChangeVerificationCommandHandler : IRequestHandler<SendEmailChangeVerificationCommand, SendEmailChangeVerificationCommandResult>
{
    private readonly IGenericRepository<EmailChangeVerification> _emailChangeVerificationRepository;
    private readonly IGenericRepository<SellerInfo> _sellerInfoRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private const int CodeExpirationMinutes = 15;
    private const int MaxAttemptsPerHour = 5;

    public SendEmailChangeVerificationCommandHandler(
        IGenericRepository<EmailChangeVerification> emailChangeVerificationRepository,
        IGenericRepository<SellerInfo> sellerInfoRepository,
        IPublishEndpoint publishEndpoint)
    {
        _emailChangeVerificationRepository = emailChangeVerificationRepository;
        _sellerInfoRepository = sellerInfoRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<SendEmailChangeVerificationCommandResult> Handle(SendEmailChangeVerificationCommand request, CancellationToken cancellationToken)
    {
        // Check if email is already in use
        var existingEmail = await _sellerInfoRepository
            .GetAllQuery()
            .AnyAsync(s => s.Email == request.NewEmail && s.SellerId != request.UserId, cancellationToken);

        if (existingEmail)
        {
            return new SendEmailChangeVerificationCommandResult
            {
                Success = false,
                Message = "Bu e-posta adresi zaten kullanılıyor."
            };
        }

        // Rate limiting
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var recentAttempts = await _emailChangeVerificationRepository
            .GetAllQuery()
            .CountAsync(p => p.UserId == request.UserId 
                          && p.CreatedDate > oneHourAgo, cancellationToken);

        if (recentAttempts >= MaxAttemptsPerHour)
        {
            return new SendEmailChangeVerificationCommandResult
            {
                Success = false,
                Message = "Çok fazla deneme yaptınız. Lütfen 1 saat sonra tekrar deneyin."
            };
        }

        // Check if there's an active verification code
        var existingCode = await _emailChangeVerificationRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId 
                                   && p.NewEmail == request.NewEmail 
                                   && !p.IsUsed 
                                   && p.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (existingCode != null)
        {
            var remainingSeconds = (int)(existingCode.ExpiresAt - DateTime.UtcNow).TotalSeconds;
            return new SendEmailChangeVerificationCommandResult
            {
                Success = false,
                Message = $"Aktif bir doğrulama kodu zaten mevcut. {remainingSeconds} saniye sonra yeni kod isteyebilirsiniz.",
                ExpiresInSeconds = remainingSeconds
            };
        }

        // Generate 6-digit code
        var code = new Random().Next(100000, 999999).ToString();

        var verification = new EmailChangeVerification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            NewEmail = request.NewEmail,
            VerificationCode = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(CodeExpirationMinutes),
            IsUsed = false,
            AttemptCount = 0,
            CreatedDate = DateTime.UtcNow
        };

        await _emailChangeVerificationRepository.AddAsync(verification, cancellationToken);

        // Send verification email via RabbitMQ
        await _publishEndpoint.Publish(new SendEmailChangeVerificationMessage
        {
            Email = request.NewEmail,
            VerificationCode = code
        }, cancellationToken);

        return new SendEmailChangeVerificationCommandResult
        {
            Message = "Doğrulama kodu e-posta adresinize gönderildi.",
            ExpiresInSeconds = CodeExpirationMinutes * 60
        }.ReturnOk();
    }
}
