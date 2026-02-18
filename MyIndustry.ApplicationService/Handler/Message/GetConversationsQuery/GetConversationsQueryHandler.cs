using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Repository.Repository;
using System.Text.Json;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainService = MyIndustry.Domain.Aggregate.Service;

namespace MyIndustry.ApplicationService.Handler.Message.GetConversationsQuery;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, GetConversationsQueryResult>
{
    private readonly IGenericRepository<DomainMessage> _messageRepository;
    private readonly IGenericRepository<DomainService> _serviceRepository;

    public GetConversationsQueryHandler(
        IGenericRepository<DomainMessage> messageRepository,
        IGenericRepository<DomainService> serviceRepository)
    {
        _messageRepository = messageRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task<GetConversationsQueryResult> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        // Get all messages where user is sender or receiver
        var messages = await _messageRepository
            .GetAllQuery()
            .Include(m => m.Service)
            .Where(m => m.SenderId == request.UserId || m.ReceiverId == request.UserId)
            .OrderByDescending(m => m.CreatedDate)
            .ToListAsync(cancellationToken);

        // Group by ServiceId + other user to create conversations
        var conversations = messages
            .GroupBy(m => new 
            { 
                m.ServiceId,
                OtherUserId = m.SenderId == request.UserId ? m.ReceiverId : m.SenderId
            })
            .Select(g => 
            {
                var lastMessage = g.First();
                var otherUserMessage = g.FirstOrDefault(m => m.SenderId != request.UserId);
                
                return new ConversationDto
                {
                    ServiceId = g.Key.ServiceId,
                    ServiceTitle = lastMessage.Service?.Title ?? "İlan",
                    ServiceImageUrl = ParseImageUrls(lastMessage.Service?.ImageUrls).FirstOrDefault(),
                    OtherUserId = g.Key.OtherUserId,
                    OtherUserName = otherUserMessage?.SenderName ?? "Kullanıcı",
                    OtherUserEmail = otherUserMessage?.SenderEmail ?? "",
                    LastMessage = lastMessage.Content.Length > 100 
                        ? lastMessage.Content.Substring(0, 100) + "..." 
                        : lastMessage.Content,
                    LastMessageDate = lastMessage.CreatedDate,
                    UnreadCount = g.Count(m => m.ReceiverId == request.UserId && !m.IsRead)
                };
            })
            .OrderByDescending(c => c.LastMessageDate)
            .ToList();

        return new GetConversationsQueryResult
        {
            Conversations = conversations
        }.ReturnOk();
    }

    private static string[] ParseImageUrls(string? imageUrls)
    {
        if (string.IsNullOrWhiteSpace(imageUrls))
            return Array.Empty<string>();
        
        try
        {
            var parsed = JsonSerializer.Deserialize<string[]>(imageUrls);
            return parsed ?? Array.Empty<string>();
        }
        catch
        {
            return imageUrls.StartsWith("http") ? new[] { imageUrls } : Array.Empty<string>();
        }
    }
}
