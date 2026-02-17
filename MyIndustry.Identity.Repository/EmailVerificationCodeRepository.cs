using Microsoft.EntityFrameworkCore;
using MyIndustry.Identity.Domain.Aggregate;
using MyIndustry.Identity.Domain.Repository;

namespace MyIndustry.Identity.Repository;

public class EmailVerificationCodeRepository : IEmailVerificationCodeRepository
{
    private readonly MyIndustryIdentityDbContext _dbContext;

    public EmailVerificationCodeRepository(MyIndustryIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(EmailVerificationCode verificationCode, CancellationToken cancellationToken)
    {
        await _dbContext.EmailVerificationCodes.AddAsync(verificationCode, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmailVerificationCode> GetLatestByUserIdAndTokenAsync(string userId, string token)
    {
        return await _dbContext.EmailVerificationCodes
            .Where(v => v.UserId == userId && v.Token == token && !v.IsUsed)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<EmailVerificationCode> GetLatestByEmailAndCodeAsync(string email, string code)
    {
        return await _dbContext.EmailVerificationCodes
            .Where(v => v.Email == email && v.Code == code && !v.IsUsed)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(EmailVerificationCode verificationCode, CancellationToken cancellationToken)
    {
        _dbContext.EmailVerificationCodes.Update(verificationCode);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
