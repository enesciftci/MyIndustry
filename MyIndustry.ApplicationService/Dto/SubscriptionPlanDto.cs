using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Dto;

public class SubscriptionPlanDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string SubscriptionType { get; set; }
    public decimal MonthlyPrice { get; set; }
    public int MonthlyPostLimit { get; set; }
    public int PostDurationInDays { get; set; }
    public int FeaturedPostLimit { get; set; }
    public bool IsActive { get; set; }
    public Guid Id { get; set; }
}