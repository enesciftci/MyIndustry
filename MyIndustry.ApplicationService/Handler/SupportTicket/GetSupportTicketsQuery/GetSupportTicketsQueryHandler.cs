using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Repository.Repository;
using DomainTicket = MyIndustry.Domain.Aggregate.SupportTicket;

namespace MyIndustry.ApplicationService.Handler.SupportTicket.GetSupportTicketsQuery;

public class GetSupportTicketsQueryHandler : IRequestHandler<GetSupportTicketsQuery, GetSupportTicketsQueryResult>
{
    private readonly IGenericRepository<DomainTicket> _ticketRepository;

    public GetSupportTicketsQueryHandler(IGenericRepository<DomainTicket> ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<GetSupportTicketsQueryResult> Handle(GetSupportTicketsQuery request, CancellationToken cancellationToken)
    {
        var query = _ticketRepository.GetAllQuery();

        // Apply filters
        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        if (request.Category.HasValue)
        {
            query = query.Where(t => t.Category == request.Category.Value);
        }

        if (request.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == request.Priority.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var tickets = await query
            .OrderByDescending(t => t.CreatedDate)
            .Skip((request.Index - 1) * request.Size)
            .Take(request.Size)
            .Select(t => new SupportTicketDto
            {
                Id = t.Id,
                UserId = t.UserId,
                UserType = t.UserType,
                Name = t.Name,
                Email = t.Email,
                Phone = t.Phone,
                Subject = t.Subject,
                Message = t.Message,
                Category = t.Category,
                Status = t.Status,
                Priority = t.Priority,
                AdminNotes = t.AdminNotes,
                AdminResponse = t.AdminResponse,
                RespondedDate = t.RespondedDate,
                ClosedDate = t.ClosedDate,
                CreatedDate = t.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return new GetSupportTicketsQueryResult
        {
            Tickets = tickets,
            TotalCount = totalCount,
            Index = request.Index,
            Size = request.Size
        }.ReturnOk();
    }
}
