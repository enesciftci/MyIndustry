using MyIndustry.Domain.Aggregate;

namespace MyIndustry.ApplicationService.Dto;

public class SupportTicketDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public int UserType { get; set; }
    public string UserTypeName => UserType switch
    {
        0 => "Anonim",
        1 => "Alıcı",
        2 => "Satıcı",
        _ => "Bilinmiyor"
    };
    public string Name { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public TicketCategory Category { get; set; }
    public string CategoryName => Category switch
    {
        TicketCategory.General => "Genel",
        TicketCategory.Technical => "Teknik",
        TicketCategory.Payment => "Ödeme",
        TicketCategory.Complaint => "Şikayet",
        TicketCategory.Suggestion => "Öneri",
        TicketCategory.Other => "Diğer",
        _ => "Bilinmiyor"
    };
    public TicketStatus Status { get; set; }
    public string StatusName => Status switch
    {
        TicketStatus.Open => "Açık",
        TicketStatus.InProgress => "İşleniyor",
        TicketStatus.Resolved => "Çözüldü",
        TicketStatus.Closed => "Kapatıldı",
        _ => "Bilinmiyor"
    };
    public TicketPriority Priority { get; set; }
    public string PriorityName => Priority switch
    {
        TicketPriority.Low => "Düşük",
        TicketPriority.Normal => "Normal",
        TicketPriority.High => "Yüksek",
        TicketPriority.Urgent => "Acil",
        _ => "Bilinmiyor"
    };
    public string? AdminNotes { get; set; }
    public string? AdminResponse { get; set; }
    public DateTime? RespondedDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
