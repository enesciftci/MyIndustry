namespace MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;

public sealed record GetServicesByFilterQuery : IRequest<GetServicesByFilterQueryResult>
{
    public Guid CategoryId { get; set; }
    // public Guid? ParentId { get; set; }

    public int? CityId { get; set; }

    public int? DistrictId { get; set; }
    // todo buraya bir enum gelmeli filter by olarak ürüne göre sırala satıcıya göre sırala olarak
}