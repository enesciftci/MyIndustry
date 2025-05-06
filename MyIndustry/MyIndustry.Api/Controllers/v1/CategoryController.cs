using MediatR;
using MyIndustry.ApplicationService.Handler.Category.CreateCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.CreateSubCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class CategoryController(IMediator mediator) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(command, cancellationToken));
    }

    [HttpGet("list")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(new GetCategoriesQuery () , cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> Update(CancellationToken cancellationToken)
    {
        return CreateResponse(null);
    }

    [HttpPost("subcategory")]
    public async Task<IActionResult> CreateSubCategory(CreateSubCategoryCommand command, CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(command, cancellationToken));
    }
}