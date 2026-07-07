using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler;
using MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;
using MyIndustry.ApplicationService.Handler.Service.DeleteServiceByIdCommand;
using MyIndustry.ApplicationService.Handler.Service.DisableServiceByIdCommand;
using MyIndustry.ApplicationService.Handler.Service.ReactivateOrExtendExpiryCommand;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByIdQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServiceBySlugQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByRandomlyQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesBySearchTermQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesBySellerIdQuery;
using MyIndustry.ApplicationService.Handler.Service.IncreaseServiceViewCountCommand;
using MyIndustry.ApplicationService.Handler.Service.UpdateServiceByIdCommand;
using MyIndustry.Api.Services;
using MyIndustry.Container.Extensions;
using MyIndustry.Domain.Aggregate.ValueObjects;
using MyIndustry.Domain.ExceptionHandling;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class ServiceController : BaseController
{
    private const int MaxTitleLength = 200;
    private const int MaxDescriptionLength = 4000;
    private const int MaxLocationLength = 100;

    private readonly IMediator _mediator;
    private readonly IImageStorageService _imageStorage;
    private readonly IImageUploadValidator _imageUploadValidator;

    public ServiceController(
        IMediator mediator,
        IImageStorageService imageStorage,
        IImageUploadValidator imageUploadValidator)
    {
        _mediator = mediator;
        _imageStorage = imageStorage;
        _imageUploadValidator = imageUploadValidator;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] int price,
        [FromForm] int estimatedDay,
        [FromForm] Guid categoryId,
        [FromForm] List<IFormFile> images,
        [FromForm] string? city,
        [FromForm] string? district,
        [FromForm] string? neighborhood,
        [FromForm] int condition = 0,
        [FromForm] int listingType = 0,
        [FromForm] bool isFeatured = false,
        CancellationToken cancellationToken = default)
    {
        ValidateServiceFields(title, description, city, district, neighborhood);

        var urls = new List<string>();
        if (images != null && images.Count > 0)
        {
            foreach (var file in images)
            {
                _imageUploadValidator.Validate(file);
                var contentType = file.ContentType ?? "image/jpeg";
                await using var stream = file.OpenReadStream();
                var url = await _imageStorage.UploadAsync(stream, file.FileName, contentType, cancellationToken);
                urls.Add(url);
            }
        }
        
        var command = new CreateServiceCommand
        {
            Title = title,
            Description = description,
            Price = price,
            EstimatedEndDay = estimatedDay,
            CategoryId = categoryId,
            SellerId = GetUserId(),
            ImageUrls = string.Join(',', urls),
            City = city,
            District = district,
            Neighborhood = neighborhood,
            Condition = (ProductCondition)condition,
            ListingType = (ListingType)listingType,
            IsFeatured = isFeatured
        };
        
        var result = await _mediator.Send(command, cancellationToken);

        return CreateResponse(result);
    }

    [HttpGet("list")]
    [Authorize]
    public async Task<IActionResult> GetServicesBySellerId([FromQuery] int index, [FromQuery] int size,
        CancellationToken cancellationToken)
    {
        var query = new GetServicesBySellerIdQuery()
        {
            SellerId = GetUserId(),
            Pager = new Pager(index,size)
        };
        
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("list/randomly")]
    public async Task<IActionResult> GetServicesByRandomly([FromQuery] int index, [FromQuery] int size,
        CancellationToken cancellationToken)
    {
        var query = new GetServicesByRandomlyQuery()
        {
            Pager = new Pager(index, size)
        };
        
        return CreateResponse(await _mediator.Send(query, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id, 
        [FromForm] string title,
        [FromForm] string description,
        [FromForm] int price,
        [FromForm] int estimatedDay,
        [FromForm] List<IFormFile> images,
        [FromForm] Guid? categoryId,
        CancellationToken cancellationToken)
    {
        ValidateServiceFields(title, description, null, null, null);

        foreach (var image in images ?? [])
            _imageUploadValidator.Validate(image);
        
        var isFeatured = false;
        if (Request.Form.TryGetValue("isFeatured", out var isFeaturedValue))
        {
            bool.TryParse(isFeaturedValue.ToString(), out isFeatured);
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
                IsFeatured = isFeatured,
                CategoryId = categoryId ?? Guid.Empty,
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

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetServiceBySlug(string slug, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new GetServiceBySlugQuery() {Slug = slug}, cancellationToken));
    }
    
    [HttpGet("filter")]
    public async Task<IActionResult> GetServiceByCategory([FromQuery] GetServicesByFilterQuery command, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// İlanı pasif yap (body yok veya reactivateOrExtendExpiry: false) veya ilanı tekrar aktif yapıp bitiş tarihini güncelle (reactivateOrExtendExpiry: true, kotadan düşer).
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> PatchService(Guid id, [FromBody] PatchServiceRequest? body, CancellationToken cancellationToken)
    {
        var sellerId = GetUserId();
        if (body?.ReactivateOrExtendExpiry == true)
        {
            return CreateResponse(await _mediator.Send(new ReactivateOrExtendExpiryCommand
            {
                ServiceId = id,
                SellerId = sellerId
            }, cancellationToken));
        }
        return CreateResponse(await _mediator.Send(new DisableServiceByIdCommand
        {
            ServiceId = id,
            SellerId = sellerId
        }, cancellationToken));
    }

    public class PatchServiceRequest
    {
        public bool ReactivateOrExtendExpiry { get; set; }
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new DeleteServiceByIdCommand()
        {
            SellerId = GetUserId(),
            Id = id
        }, cancellationToken));
    }
    
    [HttpPost("increase-viewcount/{id:guid}")]
    [EnableRateLimiting(RateLimitingExtensions.ViewCountPolicy)]
    public async Task<IActionResult> IncreaseViewCount(Guid id, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new IncreaseServiceViewCountCommand()
        {
            ServiceId = id
        }, cancellationToken));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int index, [FromQuery] int size , CancellationToken cancellationToken)
    {
        var request = new GetServicesBySearchTermQuery()
        {
            Query = query,
            Pager = new Pager(index, size)
        };
        
        return CreateResponse(await _mediator.Send(request, cancellationToken));
    }

    private static void ValidateServiceFields(
        string title,
        string description,
        string? city,
        string? district,
        string? neighborhood)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > MaxTitleLength)
            throw new BusinessRuleException($"Başlık zorunludur ve en fazla {MaxTitleLength} karakter olabilir.");

        if (string.IsNullOrWhiteSpace(description) || description.Length > MaxDescriptionLength)
            throw new BusinessRuleException($"Açıklama zorunludur ve en fazla {MaxDescriptionLength} karakter olabilir.");

        if (city?.Length > MaxLocationLength || district?.Length > MaxLocationLength || neighborhood?.Length > MaxLocationLength)
            throw new BusinessRuleException($"Konum alanları en fazla {MaxLocationLength} karakter olabilir.");
    }
}