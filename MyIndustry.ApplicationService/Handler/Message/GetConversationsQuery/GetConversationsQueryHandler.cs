using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Dto;
using MyIndustry.Repository.Repository;
using System.Text.Json;
using DomainMessage = MyIndustry.Domain.Aggregate.Message;
using DomainService = MyIndustry.Domain.Aggregate.Service;
using DomainSeller = MyIndustry.Domain.Aggregate.Seller;

namespace MyIndustry.ApplicationService.Handler.Message.GetConversationsQuery;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, GetConversationsQueryResult>
{
    private readonly IGenericRepository<DomainMessage> _messageRepository;
    private readonly IGenericRepository<DomainService> _serviceRepository;
    private readonly IGenericRepository<DomainSeller> _sellerRepository;

    public GetConversationsQueryHandler(
        IGenericRepository<DomainMessage> messageRepository,
        IGenericRepository<DomainService> serviceRepository,
        IGenericRepository<DomainSeller> sellerRepository)
    {
        _messageRepository = messageRepository;
        _serviceRepository = serviceRepository;
        _sellerRepository = sellerRepository;
    }

    public async Task<GetConversationsQueryResult> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        // Get all messages where user is sender or receiver
        var messages = await _messageRepository
            .GetAllQuery()
            .Include(m => m.Service)
                .ThenInclude(s => s.Seller)
                    .ThenInclude(seller => seller.SellerInfo)
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
                var otherUserId = g.Key.OtherUserId;
                
                // Try to get other user's name from messages they sent
                var otherUserMessage = g.FirstOrDefault(m => m.SenderId == otherUserId);
                
                // Try to get other user's name from messages where they are receiver
                var messageToOther = g.FirstOrDefault(m => m.ReceiverId == otherUserId);
                
                // Determine the other user's name:
                // 1. If other user sent a message, use their SenderName
                // 2. If we sent them a message and ReceiverName is stored, use that
                // 3. If other user is the seller, use seller title
                // 4. Default to "Kullanıcı"
                string otherUserName = "Kullanıcı";
                string otherUserEmail = "";
                
                if (otherUserMessage != null)
                {
                    // Other user sent us a message - use their sender info
                    otherUserName = otherUserMessage.SenderName ?? "Kullanıcı";
                    otherUserEmail = otherUserMessage.SenderEmail ?? "";
                }
                else if (messageToOther?.ReceiverName != null)
                {
                    // We have receiver info stored
                    otherUserName = messageToOther.ReceiverName;
                    otherUserEmail = messageToOther.ReceiverEmail ?? "";
                }
                else if (lastMessage.Service?.Seller != null && lastMessage.Service.SellerId == otherUserId)
                {
                    // Other user is the seller - use seller title
                    otherUserName = lastMessage.Service.Seller.Title ?? "Satıcı";
                    otherUserEmail = lastMessage.Service.Seller.SellerInfo?.Email ?? "";
                }
                
                return new ConversationDto
                {
                    ServiceId = g.Key.ServiceId,
                    ServiceTitle = lastMessage.Service?.Title ?? "İlan",
                    ServiceImageUrl = ParseImageUrls(lastMessage.Service?.ImageUrls).FirstOrDefault(),
                    OtherUserId = otherUserId,
                    OtherUserName = otherUserName,
                    OtherUserEmail = otherUserEmail,
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
