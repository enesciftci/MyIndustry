using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;

namespace MyIndustry.ApplicationService.Handler.Verification.VerifyEmailChangeCommand;

public sealed class VerifyEmailChangeCommandHandler : IRequestHandler<VerifyEmailChangeCommand, VerifyEmailChangeCommandResult>
{
    private readonly IGenericRepository<EmailChangeVerification> _emailChangeVerificationRepository;
    private readonly IGenericRepository<SellerInfo> _sellerInfoRepository;
    private const int MaxVerifyAttempts = 3;

    public VerifyEmailChangeCommandHandler(
        IGenericRepository<EmailChangeVerification> emailChangeVerificationRepository,
        IGenericRepository<SellerInfo> sellerInfoRepository)
    {
        _emailChangeVerificationRepository = emailChangeVerificationRepository;
        _sellerInfoRepository = sellerInfoRepository;
    }

    public async Task<VerifyEmailChangeCommandResult> Handle(VerifyEmailChangeCommand request, CancellationToken cancellationToken)
    {
        var verification = await _emailChangeVerificationRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId 
                                   && p.NewEmail == request.NewEmail 
                                   && !p.IsUsed 
                                   && p.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (verification == null)
        {
            return new VerifyEmailChangeCommandResult
            {
                Success = false,
                Message = "Geçerli bir doğrulama kodu bulunamadı. Lütfen yeni kod isteyin."
            };
        }

        // Check attempt count
        if (verification.AttemptCount >= MaxVerifyAttempts)
        {
            verification.IsUsed = true;
            _emailChangeVerificationRepository.Update(verification);
            
            return new VerifyEmailChangeCommandResult
            {
                Success = false,
                Message = "Çok fazla yanlış deneme yaptınız. Lütfen yeni kod isteyin."
            };
        }

        // Verify code
        if (verification.VerificationCode != request.Code)
        {
            verification.AttemptCount++;
            _emailChangeVerificationRepository.Update(verification);
            
            var remainingAttempts = MaxVerifyAttempts - verification.AttemptCount;
            return new VerifyEmailChangeCommandResult
            {
                Success = false,
                Message = $"Yanlış doğrulama kodu. {remainingAttempts} deneme hakkınız kaldı."
            };
        }

        // Mark as used
        verification.IsUsed = true;
        _emailChangeVerificationRepository.Update(verification);

        // Update seller email
        var sellerInfo = await _sellerInfoRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(s => s.SellerId == request.UserId, cancellationToken);

        if (sellerInfo != null)
        {
            sellerInfo.Email = request.NewEmail;
            _sellerInfoRepository.Update(sellerInfo);
        }

        return new VerifyEmailChangeCommandResult
        {
            Message = "E-posta adresi başarıyla doğrulandı ve güncellendi."
        }.ReturnOk();
    }
}
