namespace MyIndustry.Domain.Aggregate.ValueObjects;

public enum RejectionReasonType
{
    InappropriateContent = 0,      // Uygunsuz İçerik
    MissingInformation = 1,        // Eksik Bilgi
    PolicyViolation = 2,           // Politika İhlali
    DuplicateListing = 3,          // Tekrar Eden İlan
    IncorrectCategory = 4,         // Yanlış Kategori
    PriceIssue = 5,                // Fiyat Sorunu
    ImageQuality = 6,              // Görsel Kalitesi
    Other = 99                     // Diğer
}
