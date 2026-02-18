using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.Aggregate;

namespace MyIndustry.ApplicationService.Handler.Verification.VerifyPhoneCommand;

public sealed class VerifyPhoneCommandHandler : IRequestHandler<VerifyPhoneCommand, VerifyPhoneCommandResult>
{
    private readonly IGenericRepository<PhoneVerification> _phoneVerificationRepository;
    private readonly IGenericRepository<SellerInfo> _sellerInfoRepository;
    private const int MaxVerifyAttempts = 3;

    public VerifyPhoneCommandHandler(
        IGenericRepository<PhoneVerification> phoneVerificationRepository,
        IGenericRepository<SellerInfo> sellerInfoRepository)
    {
        _phoneVerificationRepository = phoneVerificationRepository;
        _sellerInfoRepository = sellerInfoRepository;
    }

    public async Task<VerifyPhoneCommandResult> Handle(VerifyPhoneCommand request, CancellationToken cancellationToken)
    {
        var verification = await _phoneVerificationRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId 
                                   && p.PhoneNumber == request.PhoneNumber 
                                   && !p.IsUsed 
                                   && p.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (verification == null)
        {
            return new VerifyPhoneCommandResult
            {
                Success = false,
                Message = "Geçerli bir doğrulama kodu bulunamadı. Lütfen yeni kod isteyin."
            };
        }

        // Check attempt count
        if (verification.AttemptCount >= MaxVerifyAttempts)
        {
            verification.IsUsed = true;
            _phoneVerificationRepository.Update(verification);
            
            return new VerifyPhoneCommandResult
            {
                Success = false,
                Message = "Çok fazla yanlış deneme yaptınız. Lütfen yeni kod isteyin."
            };
        }

        // Verify code
        if (verification.VerificationCode != request.Code)
        {
            verification.AttemptCount++;
            _phoneVerificationRepository.Update(verification);
            
            var remainingAttempts = MaxVerifyAttempts - verification.AttemptCount;
            return new VerifyPhoneCommandResult
            {
                Success = false,
                Message = $"Yanlış doğrulama kodu. {remainingAttempts} deneme hakkınız kaldı."
            };
        }

        // Mark as used
        verification.IsUsed = true;
        _phoneVerificationRepository.Update(verification);

        // Update seller phone number
        var sellerInfo = await _sellerInfoRepository
            .GetAllQuery()
            .FirstOrDefaultAsync(s => s.SellerId == request.UserId, cancellationToken);

        if (sellerInfo != null)
        {
            sellerInfo.PhoneNumber = request.PhoneNumber;
            _sellerInfoRepository.Update(sellerInfo);
        }

        return new VerifyPhoneCommandResult
        {
            Message = "Telefon numarası başarıyla doğrulandı ve güncellendi."
        }.ReturnOk();
    }
}
