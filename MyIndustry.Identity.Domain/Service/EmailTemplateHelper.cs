namespace MyIndustry.Identity.Domain.Service;

public static class EmailTemplateHelper
{
    public static string GetEmailConfirmationTemplate(string userName, string confirmationLink)
    {
        return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Email Doğrulama - MyIndustry</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7fa;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 16px; box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);"">
                    <!-- Header -->
                    <tr>
                        <td style=""padding: 40px 40px 30px; text-align: center; background: linear-gradient(135deg, #1a365d 0%, #2563eb 100%); border-radius: 16px 16px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 700; letter-spacing: -0.5px;"">
                                🏭 MyIndustry
                            </h1>
                            <p style=""margin: 10px 0 0; color: rgba(255, 255, 255, 0.85); font-size: 14px;"">
                                Endüstriyel Hizmetler Platformu
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px;"">
                            <h2 style=""margin: 0 0 20px; color: #1a365d; font-size: 24px; font-weight: 600;"">
                                Merhaba{(string.IsNullOrEmpty(userName) ? "" : $" {userName}")},
                            </h2>
                            
                            <p style=""margin: 0 0 25px; color: #4a5568; font-size: 16px; line-height: 1.6;"">
                                MyIndustry'ye hoş geldiniz! Hesabınızı aktifleştirmek için aşağıdaki butona tıklamanız yeterli:
                            </p>
                            
                            <!-- Button -->
                            <div style=""text-align: center; margin: 35px 0;"">
                                <a href=""{confirmationLink}"" style=""display: inline-block; background: linear-gradient(135deg, #2563eb 0%, #1d4ed8 100%); color: #ffffff; text-decoration: none; padding: 20px 50px; border-radius: 12px; font-size: 18px; font-weight: 600; box-shadow: 0 4px 14px rgba(37, 99, 235, 0.4);"">
                                    ✓ Hesabımı Doğrula
                                </a>
                            </div>
                            
                            <p style=""margin: 30px 0 15px; color: #64748b; font-size: 14px; line-height: 1.6; text-align: center;"">
                                Butona tıklayamıyorsanız, aşağıdaki bağlantıyı kopyalayıp tarayıcınıza yapıştırın:
                            </p>
                            <p style=""margin: 0; color: #3b82f6; font-size: 12px; word-break: break-all; text-align: center; background-color: #f1f5f9; padding: 12px; border-radius: 8px;"">
                                {confirmationLink}
                            </p>
                            
                            <div style=""background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 16px 20px; border-radius: 0 8px 8px 0; margin: 30px 0 0;"">
                                <p style=""margin: 0; color: #92400e; font-size: 14px;"">
                                    ⏰ Bu bağlantı <strong>1 saat</strong> içinde geçerliliğini yitirecektir. Süre dolarsa yeni bağlantı talep edebilirsiniz.
                                </p>
                            </div>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 30px 40px; background-color: #f8fafc; border-radius: 0 0 16px 16px;"">
                            <p style=""margin: 0 0 10px; color: #94a3b8; font-size: 13px; text-align: center;"">
                                Bu emaili siz talep etmediyseniz, lütfen dikkate almayın.
                            </p>
                            <p style=""margin: 0; color: #cbd5e1; font-size: 12px; text-align: center;"">
                                © 2026 MyIndustry. Tüm hakları saklıdır.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
    
    public static string GetPasswordResetTemplate(string userName, string resetLink)
    {
        return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Şifre Sıfırlama - MyIndustry</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7fa;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 16px; box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);"">
                    <!-- Header -->
                    <tr>
                        <td style=""padding: 40px 40px 30px; text-align: center; background: linear-gradient(135deg, #7c2d12 0%, #ea580c 100%); border-radius: 16px 16px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 700; letter-spacing: -0.5px;"">
                                🏭 MyIndustry
                            </h1>
                            <p style=""margin: 10px 0 0; color: rgba(255, 255, 255, 0.85); font-size: 14px;"">
                                Şifre Sıfırlama Talebi
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px;"">
                            <h2 style=""margin: 0 0 20px; color: #7c2d12; font-size: 24px; font-weight: 600;"">
                                Merhaba{(string.IsNullOrEmpty(userName) ? "" : $" {userName}")},
                            </h2>
                            
                            <p style=""margin: 0 0 25px; color: #4a5568; font-size: 16px; line-height: 1.6;"">
                                Şifrenizi sıfırlamak için bir talep aldık. Aşağıdaki butona tıklayarak yeni şifrenizi belirleyebilirsiniz:
                            </p>
                            
                            <!-- Warning Box -->
                            <div style=""background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 16px 20px; border-radius: 0 8px 8px 0; margin: 25px 0;"">
                                <p style=""margin: 0; color: #92400e; font-size: 14px;"">
                                    ⚠️ Bu talebi siz yapmadıysanız, bu emaili dikkate almayın ve hesabınızın güvenliğini kontrol edin.
                                </p>
                            </div>
                            
                            <!-- Button -->
                            <div style=""text-align: center; margin: 30px 0;"">
                                <a href=""{resetLink}"" style=""display: inline-block; background: linear-gradient(135deg, #ea580c 0%, #c2410c 100%); color: #ffffff; text-decoration: none; padding: 16px 40px; border-radius: 8px; font-size: 16px; font-weight: 600; box-shadow: 0 4px 14px rgba(234, 88, 12, 0.4);"">
                                    Şifremi Sıfırla
                                </a>
                            </div>
                            
                            <p style=""margin: 30px 0 0; color: #94a3b8; font-size: 13px; line-height: 1.6;"">
                                Bu link <strong style=""color: #64748b;"">1 saat</strong> içinde geçerliliğini yitirecektir.
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 30px 40px; background-color: #f8fafc; border-radius: 0 0 16px 16px;"">
                            <p style=""margin: 0; color: #cbd5e1; font-size: 12px; text-align: center;"">
                                © 2026 MyIndustry. Tüm hakları saklıdır.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
    
    public static string GetEmailChangeVerificationTemplate(string userName, string verificationCode)
    {
        return $@"
<!DOCTYPE html>
<html lang=""tr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Email Değişikliği Doğrulama - MyIndustry</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7fa;"">
    <table role=""presentation"" style=""width: 100%; border-collapse: collapse;"">
        <tr>
            <td align=""center"" style=""padding: 40px 0;"">
                <table role=""presentation"" style=""width: 600px; border-collapse: collapse; background-color: #ffffff; border-radius: 16px; box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);"">
                    <!-- Header -->
                    <tr>
                        <td style=""padding: 40px 40px 30px; text-align: center; background: linear-gradient(135deg, #065f46 0%, #10b981 100%); border-radius: 16px 16px 0 0;"">
                            <h1 style=""margin: 0; color: #ffffff; font-size: 28px; font-weight: 700; letter-spacing: -0.5px;"">
                                🏭 MyIndustry
                            </h1>
                            <p style=""margin: 10px 0 0; color: rgba(255, 255, 255, 0.85); font-size: 14px;"">
                                Email Değişikliği Doğrulama
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Content -->
                    <tr>
                        <td style=""padding: 40px;"">
                            <h2 style=""margin: 0 0 20px; color: #065f46; font-size: 24px; font-weight: 600;"">
                                Merhaba{(string.IsNullOrEmpty(userName) ? "" : $" {userName}")},
                            </h2>
                            
                            <p style=""margin: 0 0 25px; color: #4a5568; font-size: 16px; line-height: 1.6;"">
                                Email adresinizi değiştirmek için bir talep aldık. Yeni email adresinizi doğrulamak için aşağıdaki kodu kullanın:
                            </p>
                            
                            <!-- Verification Code Box -->
                            <div style=""background: linear-gradient(135deg, #ecfdf5 0%, #d1fae5 100%); border: 2px dashed #10b981; border-radius: 12px; padding: 30px; text-align: center; margin: 30px 0;"">
                                <p style=""margin: 0 0 10px; color: #64748b; font-size: 14px; text-transform: uppercase; letter-spacing: 1px;"">
                                    Doğrulama Kodunuz
                                </p>
                                <p style=""margin: 0; color: #047857; font-size: 42px; font-weight: 700; letter-spacing: 8px; font-family: 'Courier New', monospace;"">
                                    {verificationCode}
                                </p>
                            </div>
                            
                            <!-- Warning Box -->
                            <div style=""background-color: #fef3c7; border-left: 4px solid #f59e0b; padding: 16px 20px; border-radius: 0 8px 8px 0; margin: 25px 0;"">
                                <p style=""margin: 0; color: #92400e; font-size: 14px;"">
                                    ⚠️ Bu talebi siz yapmadıysanız, bu emaili dikkate almayın ve hesabınızın güvenliğini kontrol edin.
                                </p>
                            </div>
                            
                            <p style=""margin: 30px 0 0; color: #94a3b8; font-size: 13px; line-height: 1.6;"">
                                Bu kod <strong style=""color: #64748b;"">15 dakika</strong> içinde geçerliliğini yitirecektir.
                            </p>
                        </td>
                    </tr>
                    
                    <!-- Footer -->
                    <tr>
                        <td style=""padding: 30px 40px; background-color: #f8fafc; border-radius: 0 0 16px 16px;"">
                            <p style=""margin: 0; color: #cbd5e1; font-size: 12px; text-align: center;"">
                                © 2026 MyIndustry. Tüm hakları saklıdır.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
