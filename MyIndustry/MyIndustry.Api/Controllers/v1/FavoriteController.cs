using MediatR;
using Microsoft.AspNetCore.Authorization;
using MyIndustry.ApplicationService.Handler.Favorite.AddFavoriteCommand;
using MyIndustry.ApplicationService.Handler.Favorite.DeleteFavoriteCommand;
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
        var result = await _mediator.Send(command, cancellationToken);
        return CreateResponse(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFavorite(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteFavoriteCommand { Id = id }, cancellationToken);
        return CreateResponse(result);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetFavorite(Guid serviceId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFavoriteQuery()
        {
            UserId = GetUserId(),
            ServiceId = serviceId
        }, cancellationToken);
        
        return CreateResponse(result);
    }
}