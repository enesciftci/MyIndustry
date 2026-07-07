using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace MyIndustry.Api.Controllers.v1;

// Stub / devre dışı: Komisyon API ileride kullanılacaksa route ve [Authorize] açılmalı (yetkisiz erişim engellenmeli).
// [Route("api/v{version:apiVersion}/[controller]s")]
// [ApiController]
// [Authorize]
// public class CommissionController : BaseController
// {
//     private readonly IMediator _mediator;
//
//     public CommissionController(IMediator mediator)
//     {
//         _mediator = mediator;
//     }
//
//     [HttpPost]
//     public async Task<IActionResult> Create(CancellationToken cancellationToken)
//     {
//         return CreateResponse(null);
//     }
//
//     [HttpGet]
//     public async Task<IActionResult> Get(CancellationToken cancellationToken)
//     {
//         return CreateResponse(null);
//     }
//
//     [HttpPut]
//     public async Task<IActionResult> Update(CancellationToken cancellationToken)
//     {
//         return CreateResponse(null);
//     }
// }