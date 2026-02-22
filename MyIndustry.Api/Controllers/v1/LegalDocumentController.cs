using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.ApplicationService.Handler.LegalDocument.CreateLegalDocumentCommand;
using MyIndustry.ApplicationService.Handler.LegalDocument.DeleteLegalDocumentCommand;
using MyIndustry.ApplicationService.Handler.LegalDocument.GetActiveLegalDocumentsByTypesQuery;
using MyIndustry.ApplicationService.Handler.LegalDocument.GetAllLegalDocumentsQuery;
using MyIndustry.ApplicationService.Handler.LegalDocument.GetLegalDocumentByIdQuery;
using MyIndustry.ApplicationService.Handler.LegalDocument.GetLegalDocumentByTypeQuery;
using MyIndustry.ApplicationService.Handler.LegalDocument.UpdateLegalDocumentCommand;

namespace MyIndustry.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:ApiVersion}/[controller]")]
public class LegalDocumentController : BaseController
{
    private readonly IMediator _mediator;

    public LegalDocumentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Kayıt sayfası ve genel kullanım için aktif sözleşmeleri döner (herkese açık).
    /// </summary>
    [HttpGet("public/for-registration")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveForRegistration([FromQuery] List<int>? types, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetActiveLegalDocumentsByTypesQuery { DocumentTypes = types }, cancellationToken);
        return CreateResponse(result);
    }

    /// <summary>
    /// Tipine göre tek aktif sözleşmeyi döner (örn. /terms, /privacy sayfaları için). Herkese açık.
    /// </summary>
    [HttpGet("public/by-type/{documentType:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByType(int documentType, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLegalDocumentByTypeQuery { DocumentType = documentType }, cancellationToken);
        return CreateResponse(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        if (!IsAdmin())
            return Unauthorized();

        var result = await _mediator.Send(new GetAllLegalDocumentsQuery(), cancellationToken);
        return CreateResponse(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
            return Unauthorized();

        var result = await _mediator.Send(new GetLegalDocumentByIdQuery { Id = id }, cancellationToken);
        return CreateResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LegalDocumentDto legalDocumentDto, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
            return Unauthorized();

        var result = await _mediator.Send(new CreateLegalDocumentCommand
        {
            LegalDocumentDto = legalDocumentDto
        }, cancellationToken);
        return CreateResponse(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] LegalDocumentDto legalDocumentDto, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
            return Unauthorized();

        var result = await _mediator.Send(new UpdateLegalDocumentCommand
        {
            LegalDocumentDto = legalDocumentDto
        }, cancellationToken);
        return CreateResponse(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
            return Unauthorized();

        var result = await _mediator.Send(new DeleteLegalDocumentCommand { Id = id }, cancellationToken);
        return CreateResponse(result);
    }
}
