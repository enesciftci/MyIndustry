namespace MyIndustry.ApplicationService.Handler;

public record PagerResponseBase : ResponseBase
{
    public Pager Pager { get; set; }
}