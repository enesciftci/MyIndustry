namespace MyIndustry.Domain.Aggregate.ValueObjects;

public enum SuspensionReasonType
{
    PolicyViolation = 0,           // Politika İhlali
    InappropriateContent = 1,      // Uygunsuz İçerik
    SpamOrFraud = 2,               // Spam veya Dolandırıcılık
    CopyrightInfringement = 3,      // Telif Hakkı İhlali
    UserComplaint = 4,             // Kullanıcı Şikayeti
    TermsOfServiceViolation = 5,   // Hizmet Şartları İhlali
    TemporaryMaintenance = 6,      // Geçici Bakım
    Other = 99                     // Diğer
}
