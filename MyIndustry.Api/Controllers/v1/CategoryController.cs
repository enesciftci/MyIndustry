using MediatR;
using MyIndustry.ApplicationService.Handler.Category.CreateCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.CreateSubCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;
using MyIndustry.ApplicationService.Handler.Category.GetMainCategoriesQuery;

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
    public async Task<IActionResult> Get([FromQuery] Guid? parentId, CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(new GetCategoriesQuery ()
        {
            ParentId = parentId
        } , cancellationToken));
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

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree(CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(new GetCategoriesQuery2(), cancellationToken));
    }
    
    [HttpGet("main")]
    public async Task<IActionResult> GetMainCategories(CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(new GetMainCategoriesQuery(), cancellationToken));
    }
    
    [HttpGet("{parentId:guid}")]
    public async Task<IActionResult> GetSubCategories(Guid parentId, CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(new GetCategoriesQuery2(){ParentId =parentId }, cancellationToken));
    }
}