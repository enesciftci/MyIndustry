namespace MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;

public record GetCategoriesQuery :  IRequest<GetCategoriesQueryResult>
{
    public Guid? ParentId { get; set; }
}

public record GetCategoriesQuery2 :  IRequest<GetCategoriesQueryResult>
{
    public Guid? ParentId { get; set; }
}
