using MediatR;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;
using MyIndustry.ApplicationService.Handler.Service.DeleteServiceByIdCommand;
using MyIndustry.ApplicationService.Handler.Service.DisableServiceByIdCommand;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByIdQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByRandomlyQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesBySellerIdQuery;
using MyIndustry.ApplicationService.Handler.Service.IncreaseServiceViewCountCommand;
using MyIndustry.ApplicationService.Handler.Service.UpdateServiceByIdCommand;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class ServiceController : BaseController
{
    private readonly IMediator _mediator;

    public ServiceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] int price,
        [FromForm] int estimatedDay,
        [FromForm] Guid categoryId,
        [FromForm] Guid subCategoryId,
        [FromForm] List<IFormFile> images,
        CancellationToken cancellationToken)
    {
        var command = new CreateServiceCommand
        {
            Title = title,
            Description = description,
            Price = price,
            EstimatedEndDay = estimatedDay,
            CategoryId = categoryId,
            SubCategoryId = subCategoryId,
            SellerId = GetUserId()
            // ImageUrls = images
        };

        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetServicesBySellerId([FromQuery] GetServicesBySellerIdQuery query,
        CancellationToken cancellationToken)
    {
        query.SellerId = GetUserId();
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("list/randomly")]
    public async Task<IActionResult> GetServicesByRandomly([FromQuery] GetServicesByRandomlyQuery query,
        CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id, 
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] int price,
        [FromForm] int estimatedDay,
        [FromForm] List<IFormFile> images,
        CancellationToken cancellationToken)
    {
        foreach (var image in images)
        {
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms, cancellationToken);
            var imageBytes = ms.ToArray();

            // Burada her bir imageBytes ile işlem yapabilirsin
        }
        
        var command = new UpdateServiceByIdCommand
        {
            ServiceDto = new ServiceDto
            {
                Id = id,
                Title = title,
                Description = description,
                Price = price,
                EstimatedEndDay = estimatedDay,
                // ImageUrls = image,
                SellerId = GetUserId()
            }
        };
        
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetServiceById(Guid id, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new GetServicesByIdQuery() {Id = id}, cancellationToken));
    }
    
    [HttpGet("filter")]
    public async Task<IActionResult> GetServiceByCategory([FromQuery] GetServicesByFilterQuery command, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> DisableServiceById(Guid id, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new DisableServiceByIdCommand()
        {
            SellerId = GetUserId(),
            ServiceId = id
        }, cancellationToken));
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new DeleteServiceByIdCommand()
        {
            SellerId = GetUserId(),
            Id = id
        }, cancellationToken));
    }
    
    [HttpPost("increase-viewcount/{id:guid}")]
    public async Task<IActionResult> IncreaseViewCount(Guid id, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new IncreaseServiceViewCountCommand()
        {
            ServiceId = id
        }, cancellationToken));
    }
}