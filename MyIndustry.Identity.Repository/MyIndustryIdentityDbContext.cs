using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyIndustry.Identity.Domain.Aggregate;

namespace MyIndustry.Identity.Repository;

public class MyIndustryIdentityDbContext : IdentityDbContext<ApplicationUser>
{
    public MyIndustryIdentityDbContext(DbContextOptions<MyIndustryIdentityDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        builder.Entity<EmailVerificationCode>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(6).IsRequired();
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(e => new { e.Email, e.Code });
            entity.HasIndex(e => e.UserId);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}