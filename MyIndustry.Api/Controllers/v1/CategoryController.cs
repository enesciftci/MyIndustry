using MediatR;
using MyIndustry.ApplicationService.Handler.Category.CreateCategoryCommand;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class CategoryController(IMediator mediator) : BaseController
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        return CreateResponse(null);
    }

    [HttpPut]
    public async Task<IActionResult> Update(CancellationToken cancellationToken)
    {
        return CreateResponse(null);
    }
}