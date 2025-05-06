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
}