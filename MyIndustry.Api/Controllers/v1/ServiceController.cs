using MediatR;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler;
using MyIndustry.ApplicationService.Handler.Service.CreateServiceCommand;
using MyIndustry.ApplicationService.Handler.Service.DeleteServiceByIdCommand;
using MyIndustry.ApplicationService.Handler.Service.DisableServiceByIdCommand;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByFilterQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByIdQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServiceBySlugQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesByRandomlyQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesBySearchTermQuery;
using MyIndustry.ApplicationService.Handler.Service.GetServicesBySellerIdQuery;
using MyIndustry.ApplicationService.Handler.Service.IncreaseServiceViewCountCommand;
using MyIndustry.ApplicationService.Handler.Service.UpdateServiceByIdCommand;
using MyIndustry.Domain.Aggregate.ValueObjects;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class ServiceController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ServiceController> _logger;


    public ServiceController(IMediator mediator, IWebHostEnvironment env, ILogger<ServiceController> logger)
    {
        _mediator = mediator;
        _env = env;
        _logger = logger;
    }

    [HttpPost]
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
        _logger.LogInformation("Creating service: Title={Title}, CategoryId={CategoryId}, SellerId={SellerId}, ImageCount={ImageCount}", 
            title, categoryId, GetUserId(), images?.Count ?? 0);
        
        var urls = new List<string>();

        // Determine the uploads directory - use /app/wwwroot/uploads in production
        var uploadsPath = _env.WebRootPath != null 
            ? Path.Combine(_env.WebRootPath, "uploads")
            : Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
        
        _logger.LogInformation("WebRootPath={WebRootPath}, ContentRootPath={ContentRootPath}, UploadsPath={UploadsPath}", 
            _env.WebRootPath ?? "null", _env.ContentRootPath, uploadsPath);
        
        // Fallback to /tmp/uploads if the standard path doesn't exist or isn't writable
        if (!Directory.Exists(uploadsPath))
        {
            _logger.LogWarning("Uploads path does not exist, attempting to create: {UploadsPath}", uploadsPath);
            try
            {
                Directory.CreateDirectory(uploadsPath);
                _logger.LogInformation("Created uploads directory: {UploadsPath}", uploadsPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create uploads directory at {UploadsPath}, falling back to /tmp/uploads", uploadsPath);
                // If we can't create in the standard location, use /tmp
                uploadsPath = "/tmp/uploads";
                Directory.CreateDirectory(uploadsPath);
                _logger.LogInformation("Using fallback uploads directory: {UploadsPath}", uploadsPath);
            }
        }

        if (images != null && images.Count > 0)
        {
            foreach (var file in images)
            {
                _logger.LogInformation("Processing file: {FileName}, Size={Size}", file.FileName, file.Length);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }
                _logger.LogInformation("File saved: {FilePath}", filePath);

                var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
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
        
        _logger.LogInformation("Sending CreateServiceCommand: SellerId={SellerId}, CategoryId={CategoryId}", command.SellerId, command.CategoryId);
        
        var result = await _mediator.Send(command, cancellationToken);
        
        _logger.LogInformation("CreateServiceCommand completed: Success={Success}", result?.Success);
        
        return CreateResponse(result);
    }

    [HttpGet("list")]
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
}