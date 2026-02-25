using MediatR;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler.Category.CreateCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.CreateSubCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.DeleteCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;
using MyIndustry.ApplicationService.Handler.Category.GetMainCategoriesQuery;
using MyIndustry.ApplicationService.Handler.Category.UpdateCategoryCommand;

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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        };
        return CreateResponse(await mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(new DeleteCategoryCommand { Id = id }, cancellationToken));
    }

    [HttpPost("subcategory")]
    public async Task<IActionResult> CreateSubCategory([FromBody] CreateSubCategoryRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();
        var parentId = request.ParentId != Guid.Empty ? request.ParentId : request.CategoryId;
        var command = new CreateSubCategoryCommand(new SubCategoryDto
        {
            CategoryId = parentId,
            Name = request.Name ?? "",
            Description = request.Description ?? ""
        });
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

public class UpdateCategoryRequest
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Flat body for POST /categories/subcategory. Frontend sends parentId, name, description.</summary>
public class CreateSubCategoryRequest
{
    public Guid ParentId { get; set; }
    /// <summary>Alias for ParentId (backend previously expected subCategory.categoryId).</summary>
    public Guid CategoryId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}