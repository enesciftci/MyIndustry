using MyIndustry.Identity.Domain.Aggregate;

namespace MyIndustry.Identity.Domain.Repository;

public interface IEmailVerificationCodeRepository
{
    Task AddAsync(EmailVerificationCode verificationCode, CancellationToken cancellationToken);
    Task<EmailVerificationCode> GetLatestByUserIdAndTokenAsync(string userId, string token);
    Task<EmailVerificationCode> GetLatestByEmailAndCodeAsync(string email, string code);
    Task UpdateAsync(EmailVerificationCode verificationCode, CancellationToken cancellationToken);
}
