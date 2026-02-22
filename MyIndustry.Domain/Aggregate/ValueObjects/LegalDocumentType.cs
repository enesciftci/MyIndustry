namespace MyIndustry.Domain.Aggregate.ValueObjects;

public enum LegalDocumentType
{
    KVKK = 1,                    // Kişisel Verilerin Korunması Kanunu
    MembershipAgreement = 2,      // Üyelik Sözleşmesi
    TermsOfService = 3,          // Kullanım Şartları
    PrivacyPolicy = 4,           // Gizlilik Politikası
    CookiePolicy = 5,            // Çerez Politikası
    RefundPolicy = 6,            // İade Politikası
    SellerAgreement = 7,         // Satıcı Sözleşmesi
    Other = 99                   // Diğer
}
