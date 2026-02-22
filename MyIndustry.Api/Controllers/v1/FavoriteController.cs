using MediatR;
using Microsoft.AspNetCore.Authorization;
using MyIndustry.ApplicationService.Handler.Favorite.AddFavoriteCommand;
using MyIndustry.ApplicationService.Handler.Favorite.DeleteFavoriteCommand;
using MyIndustry.ApplicationService.Handler.Favorite.GetFavoriteListQuery;
using MyIndustry.ApplicationService.Handler.Favorite.GetFavoriteQuery;

namespace MyIndustry.Api.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v{version:ApiVersion}/[controller]s")]
public class FavoriteController : BaseController
{
    private readonly IMediator _mediator;

    public FavoriteController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteCommand command, CancellationToken cancellationToken)
    {
        command.UserId = GetUserId();
        var result = await _mediator.Send(command, cancellationToken);
        return CreateResponse(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFavorite([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new DeleteFavoriteCommand 
        { 
            FavoriteId = id,
            ServiceId = Guid.Empty, // Only use FavoriteId for deletion
            UserId = userId
        }, cancellationToken);
        return CreateResponse(result);
    }
    
    [HttpGet("{serviceId:guid}")]
    public async Task<IActionResult> GetFavorite([FromRoute] Guid serviceId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFavoriteQuery()
        {
            UserId = GetUserId(),
            ServiceId = serviceId
        }, cancellationToken);
        
        return CreateResponse(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetFavoriteList(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetFavoriteListQuery()
        {
            UserId = userId
        }, cancellationToken);
        
        return CreateResponse(result);
    }
}