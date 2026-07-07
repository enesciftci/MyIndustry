using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler.Category.CreateCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.CreateSubCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.DeleteCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;
using MyIndustry.ApplicationService.Handler.Category.GetMainCategoriesQuery;
using MyIndustry.ApplicationService.Handler.Category.UpdateCategoryCommand;
using MyIndustry.Container.Logging;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class CategoryController(IMediator mediator, ILogger<CategoryController> logger) : BaseController
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<CategoryController> _logger = logger;

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        AdminAuditLogger.LogAdminAction(_logger, "CreateCategory", GetUserId().ToString(), command.Name,
            HttpContext.Items[CorrelationIdConstants.ItemKey]?.ToString(), new { command.Name });
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("list")]
    public async Task<IActionResult> Get([FromQuery] Guid? parentId, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new GetCategoriesQuery ()
        {
            ParentId = parentId
        } , cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive
        };
        AdminAuditLogger.LogAdminAction(_logger, "UpdateCategory", GetUserId().ToString(), id.ToString(),
            HttpContext.Items[CorrelationIdConstants.ItemKey]?.ToString(), new { request.Name, request.IsActive });
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        AdminAuditLogger.LogAdminAction(_logger, "DeleteCategory", GetUserId().ToString(), id.ToString(),
            HttpContext.Items[CorrelationIdConstants.ItemKey]?.ToString());
        return CreateResponse(await _mediator.Send(new DeleteCategoryCommand { Id = id }, cancellationToken));
    }

    [HttpPost("subcategory")]
    [Authorize(Policy = "AdminOnly")]
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
        AdminAuditLogger.LogAdminAction(_logger, "CreateSubCategory", GetUserId().ToString(), parentId.ToString(),
            HttpContext.Items[CorrelationIdConstants.ItemKey]?.ToString(), new { request.Name, ParentId = parentId });
        return CreateResponse(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree(CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new GetCategoriesQuery2(), cancellationToken));
    }
    
    [HttpGet("main")]
    public async Task<IActionResult> GetMainCategories(CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new GetMainCategoriesQuery(), cancellationToken));
    }
    
    [HttpGet("{parentId:guid}")]
    public async Task<IActionResult> GetSubCategories(Guid parentId, CancellationToken cancellationToken)
    {
        return CreateResponse(await _mediator.Send(new GetCategoriesQuery2(){ParentId =parentId }, cancellationToken));
    }
}

public class UpdateCategoryRequest
{
    [StringLength(200)]
    public string Name { get; set; } = "";
    [StringLength(1000)]
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Flat body for POST /categories/subcategory. Frontend sends parentId, name, description.</summary>
public class CreateSubCategoryRequest
{
    public Guid ParentId { get; set; }
    /// <summary>Alias for ParentId (backend previously expected subCategory.categoryId).</summary>
    public Guid CategoryId { get; set; }
    [StringLength(200)]
    public string? Name { get; set; }
    [StringLength(1000)]
    public string? Description { get; set; }
}
