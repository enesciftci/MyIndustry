# MyIndustry Güvenlik İncelemesi

Bu belge kod tabanında yapılan güvenlik odaklı incelemenin bulgularını ve yapılan/yapılması gereken iyileştirmeleri özetler.

---

## Yapılan Düzeltmeler

### 1. Admin yetkisi (Authorization)
- **Sorun:** `AdminController` (Main API) ve Identity `AdminController` sadece `[Authorize]` kullanıyordu; herhangi bir giriş yapmış kullanıcı admin endpoint’lerini çağarabiliyordu.
- **Çözüm:** `AdminOnly` policy eklendi (claim `type` == `"99"`). Hem Main API hem Identity API’deki admin controller’lar `[Authorize(Policy = "AdminOnly")]` ile güncellendi.
- **Dosyalar:** `MyIndustry.Api/Program.cs`, `MyIndustry.Api/Controllers/v1/AdminController.cs`, `MyIndustry.Identity.Api/Program.cs`, `MyIndustry.Identity.Api/Controllers/AdminController.cs`.

### 2. Destek talebi (Support Ticket) güncellemesi
- **Sorun:** `SupportTicketController` GetAll ve Update yalnızca “admin only” diye belgelenmişti ama policy yoktu; her giriş yapmış kullanıcı tüm biletleri listeleyip güncelleyebiliyordu.
- **Çözüm:** Bu iki endpoint `[Authorize(Policy = "AdminOnly")]` ile korundu.
- **Dosyalar:** `MyIndustry.Api/Controllers/v1/SupportTicketController.cs`.

### 3. Internal API key karşılaştırması
- **Sorun:** `InternalController` içinde `X-Internal-Api-Key` için `!=` ile string karşılaştırması yapılıyordu; timing side-channel ile anahtar tahmin edilebilirdi.
- **Çözüm:** `CryptographicOperations.FixedTimeEquals` ile sabit süreli karşılaştırma kullanıldı.
- **Dosya:** `MyIndustry.Api/Controllers/v1/InternalController.cs`.

### 4. ForgotPassword Open Redirect
- **Sorun:** `ClientUrl` isteğin body’sinden alınıp doğrudan şifre sıfırlama linkinde kullanılıyordu; saldırgan kendi alan adını vererek kurbanı phishing sayfasına yönlendirebilirdi.
- **Çözüm:** Sadece config’teki izin verilen base URL kabul ediliyor: `PasswordReset:AllowedBaseUrl` veya `FrontendUrl`. Eşleşmezse config’teki varsayılan kullanılıyor.
- **Dosyalar:** `MyIndustry.Identity.Domain/Service/UserService.cs`, `MyIndustry.Identity.Api/appsettings.json`.

### 5. JWT imza anahtarı ve admin şifresi
- **Sorun:** JWT signing key ve admin seed şifresi kod içinde sabit tanımlıydı.
- **Çözüm:**
  - JWT anahtarı: `Jwt:SigningKey` config’ten (veya env: `Jwt__SigningKey`) okunuyor. Development’ta boşsa yerel geliştirme için bir placeholder kullanılıyor; Production’da boşsa uygulama başlamıyor.
  - Admin şifresi: `SeedAdmin:Password` veya `ADMIN_PASSWORD` env. Boşsa admin kullanıcı seed’i atlanıyor.
- **Dosyalar:** `MyIndustry.Identity.Domain/Service/AuthService.cs`, `MyIndustry.Identity.Api/Program.cs`, `MyIndustry.Api/Program.cs`, `appsettings.json` (her iki API).

### 6. CORS (config-driven)
- **Durum:** Önceden her zaman `AllowAnyOrigin()` kullanılıyordu.
- **Çözüm:** `Cors:AllowedOrigins` config’ten (string array) okunuyor. Liste **doluysa** yalnızca bu origin’lere izin veriliyor; **boşsa** (Development dahil) davranış eskisi gibi `AllowAnyOrigin`. Production’da frontend adresinizi ekleyin (örn. `["https://myindustry.com","https://www.myindustry.com"]`).
- **Dosyalar:** `MyIndustry.Api/Program.cs`, `MyIndustry.Identity.Api/Program.cs`, `appsettings.json`.

### 7. HTTPS ve reverse proxy
- **Durum:** `UseHttpsRedirection()` kapalıydı.
- **Çözüm:** Production’da (`!IsDevelopment`) `UseHttpsRedirection()` açıldı. Reverse proxy (Nginx, Dokploy vb.) kullanıyorsanız `UseForwardedHeaders` eklendi; `X-Forwarded-Proto` ve `X-Forwarded-For` işleniyor, böylece proxy HTTPS’i sonlandırsa bile uygulama doğru scheme’i görüyor. Varsayılan olarak sadece loopback proxy kabul edilir; proxy ayrı sunucudaysa `ForwardedHeadersOptions.KnownProxies` ile IP ekleyin.
- **Dosyalar:** `MyIndustry.Api/Program.cs`, `MyIndustry.Identity.Api/Program.cs`.

### 8. Input validasyonu
- **Destek talebi:** `CreateTicketRequest` için `[Required]`, `[EmailAddress]`, `[StringLength]` eklendi (Name, Email, Subject, Message). `UpdateTicketRequest` için AdminNotes/AdminResponse `[StringLength(2000/4000)]`. Create endpoint’inde `ModelState.IsValid` kontrolü eklendi.
- **Admin:** `ApproveListingRequest` ve `SuspendRequest` için açıklama alanları `[StringLength(1000)]`. Listings/GetAll için `index` ve `size` `[Range(1, 1000)]` / `[Range(1, 100)]`.
- **Internal:** `LegalDocumentIds` listesi en fazla 100 öğe ile sınırlandı (`Take(100)`).
- **Dosyalar:** `SupportTicketController.cs`, `AdminController.cs`, `InternalController.cs`.

---

## Yapılması Gerekenler (Öneriler)

### Yapılandırma ve ortam
- **Production’da mutlaka ayarlayın:**
  - `Jwt:SigningKey`: Güçlü, rastgele bir anahtar (en az 32 karakter). Main API ve Identity API’de aynı olmalı.
  - `InternalApiKey`: Güçlü, tahmin edilemez bir değer.
  - `SeedAdmin:Password` veya `ADMIN_PASSWORD`: İlk admin hesabı için (sadece seed sırasında).
- **Şifre sıfırlama linki:** Production’da `PasswordReset:AllowedBaseUrl` veya `FrontendUrl`’i gerçek frontend adresinize ayarlayın.

**Production checklist (deploy öncesi):** `Jwt:SigningKey`, `InternalApiKey`, `Cors:AllowedOrigins`, `SeedAdmin:Password`/`ADMIN_PASSWORD`, `PasswordReset:AllowedBaseUrl`/`FrontendUrl`, Redis connection (blacklist için).

### CORS
- **Durum:** `Cors:AllowedOrigins` config’e bağlandı. Liste doluysa yalnızca o origin’ler kabul edilir; boşsa `AllowAnyOrigin` (önceki davranış).
- **Yapmanız gereken:** Production’da `appsettings` veya ortam değişkeni ile `Cors:AllowedOrigins` listesine frontend origin’lerinizi ekleyin (örn. `["https://myindustry.com"]`). Böylece sadece bilinen origin’lerden gelen istekler CORS’ta kabul edilir.

### HTTPS
- **Durum:** Production’da `UseHttpsRedirection()` açık; reverse proxy için `UseForwardedHeaders` (X-Forwarded-Proto / X-Forwarded-For) eklendi.
- **KnownProxies:** Proxy ayrı makinedeyse config’ten `ForwardedHeaders:KnownProxies` (IP adresi listesi) ile proxy IP’leri eklenebilir; boşsa sadece loopback kabul edilir.

### Input validasyonu
- **Durum:** Destek talebi (Create/Update), Admin (Approve/Suspend, listeleme sayfalama) ve Internal (legal document listesi) için validasyon/sınırlar eklendi (bakınız “Yapılan Düzeltmeler” 8).
- **Genişletildi:** Kategori (UpdateCategoryRequest, CreateSubCategoryRequest) için `[StringLength]`; LegalDocumentDto için Title/Content/Version `[StringLength]`; mesaj istekleri (SendMessageRequest, ReplyMessageRequest) için Content `[StringLength(2000)]` eklendi.

### Hassas veri loglama
- **Durum:** `ServiceController.Create` artık hassas alan (title, categoryId, sellerId, dosya adı) loglamıyor; sadece `ImageCount` ve `Success` loglanıyor. `EnableSensitiveDataLogging` yalnızca Development’ta açık (zaten öyle).

### Diğer
- **JWT blacklist:** Logout sonrası token'lar Redis'te `jwt_blacklist:{jti}` ile TTL = token süresi dolana kadar saklanır. Identity API ve Main API, her istekte bu listeyi kontrol eder; blacklist'teyse 401 döner. Main API'de blacklist kontrolü için **Redis bağlantısı** gerekir (`ConnectionStrings:Redis`). Redis yoksa Main API blacklist kontrolü yapmaz (token süresi dolana kadar geçerli kalır).
- **SellerController.Update:** Düzeltildi – artık `UpdateSellerCommand` mevcut kullanıcı için çalıştırılıyor (`Id = GetUserId()`), `[Authorize]` eklendi.
- **CommissionController:** Açıklama eklendi: yeniden etkinleştirilirse `[Authorize]` mutlaka açılmalı.
- **FromSqlRaw:** `GenericRepository` içinde güvenlik uyarısı yorumu eklendi; parametreli sorgu kullanılması öneriliyor.
- **Hassas veri loglama:** `ServiceController.Create` loglarında artık yalnızca `ImageCount` ve `Success` loglanıyor; title, categoryId, sellerId ve dosya adı kaldırıldı.

---

## Özet

| Konu | Önceki durum | Şu an |
|------|----------------|--------|
| Admin endpoint’leri | Her giriş yapmış kullanıcı erişebiliyordu | Sadece `type` claim’i 99 olan (Admin) erişebilir |
| Destek talebi listeleme/güncelleme | Her giriş yapmış kullanıcı | Sadece admin |
| Internal API key | String `!=` (timing riski) | Sabit süreli karşılaştırma |
| ForgotPassword redirect | İstekten gelen URL kullanılıyordu | Sadece config’teki base URL kullanılıyor |
| JWT key / admin şifresi | Kodda sabit | Config/env; Production’da zorunlu/boşsa seed yok |
| CORS | Her zaman AllowAnyOrigin | Config: Cors:AllowedOrigins doluysa sadece o origin’ler |
| HTTPS | Kapalı | Production’da açık; ForwardedHeaders ile proxy uyumlu |
| Request validasyonu | Sınırlı | Destek/Admin/Internal için uzunluk ve range kuralları |

Güvenlikle ilgili ek soru veya inceleme isterseniz bu belgeyi temel alarak devam edebilirsiniz.

---

## Fazlı Güvenlik İyileştirmeleri (2026)

### Faz 1 — Kritik erişim kontrolü
- Kayıtta `UserType` whitelist: yalnızca `User` ve `Seller`; `Admin` reddedilir.
- `CategoryController` mutasyonları `AdminOnly` policy ile korunur.
- `SubscriptionPlanController.Create` `AdminOnly` policy ile korunur.
- `LegalDocumentController` manuel `IsAdmin()` yerine `AdminOnly` policy kullanır.
- Gateway sabit JWT anahtarı kaldırıldı; `Jwt:SigningKey` config/env'den okunur.

### Faz 2 — Auth sertleştirme
- Login Redis cache şifresiz token dönüşü kaldırıldı.
- `ServiceController` ve `SellerSubscriptionController` hassas endpoint'lere `[Authorize]` eklendi.
- `ExceptionHandlingMiddleware`: `UnauthorizedAccessException` → 401; production 500 generic mesaj.
- JWT `ValidateIssuer`, `Jwt:Issuer`, `RequireHttpsMetadata` (production).
- Rate limiting: auth endpoint'leri (Identity API).
- Production'da Main API Redis zorunlu (JWT blacklist).

### Faz 3 — Upload ve input
- `ImageUploadValidator`: boyut, uzantı, magic byte kontrolü.
- `/uploads` static serve: `X-Content-Type-Options: nosniff`.
- Service alanları için uzunluk validasyonu.
- `FromSqlRaw` `[Obsolete]` işaretlendi.

### Faz 4 — CAPTCHA, destek, abonelik
- reCAPTCHA backend doğrulaması (`Recaptcha:SecretKey`).
- Destek talebi IP rate limit.
- `subscribe`/`upgrade` geçici olarak `AdminOnly` (ödeme entegrasyonu sonrası açılacak).
- Pending phone/email Redis + TTL.
- Doğrulama kodu attempt limit (Redis).

### Faz 5 — Headers ve config
- `SecurityHeadersMiddleware` (tüm API'ler + Gateway).
- `RESET_DATABASE` / `RESET_IDENTITY_DATABASE` production'da ignore.
- `SeedAdmin:Email` / `ADMIN_EMAIL` config desteği.
- Gateway HTTPS + ForwardedHeaders.
- Refresh token reuse detection.

### Kalan maddeler (2026 tamamlama)
- **Admin audit log:** `CategoryController`, `SubscriptionPlanController`, `SEOController` (generate-slugs) mutasyonları `AdminAuditLogger` ile loglanır.
- **AllowedHosts:** Production startup guard (`AllowedHostsExtensions`); şablonlar `MyIndustry.Container/ProductionAllowedHosts.*.json`.
- **Docker ağ izolasyonu:** Varsayılan `compose.yaml` API/Identity host portlarını açmaz; debug için `--profile debug`. Bkz. [`COMPOSE.md`](COMPOSE.md).

### Production checklist (güncel)

| Değişken | Servis |
|----------|--------|
| `Jwt__SigningKey` | Api, Identity, Gateway |
| `Jwt__Issuer` | Api, Identity, Gateway |
| `InternalApiKey` | Api |
| `Cors__AllowedOrigins__0` | Api, Identity, Gateway |
| `REDIS_HOST` | Api, Identity |
| `REDIS_PASSWORD` | Api, Identity |
| `ConnectionStrings__Redis` | Api, Identity (compose tarafından türetilir; elle set etmeyin) |
| `Recaptcha__SecretKey` | Api |
| `SeedAdmin__Password` / `ADMIN_PASSWORD` | Identity |
| `SeedAdmin__Email` / `ADMIN_EMAIL` | Identity |
| `PasswordReset__AllowedBaseUrl` | Identity |
| `ForwardedHeaders__KnownProxies__0` | Api, Identity, Gateway |
| `AllowedHosts` veya `AllowedHosts__0` | Api, Identity, Gateway |

**AllowedHosts (Production zorunlu):** Startup guard production'da `AllowedHosts=*` veya boş değerle uygulamayı başlatmaz. Deploy sırasında gerçek domain(ler)inizi ayarlayın:

```bash
# Tek domain
AllowedHosts__0=api.myindustry.com

# Birden fazla domain (noktalı virgül ile)
AllowedHosts=api.myindustry.com;gateway.myindustry.com
```

Şablon dosyalar: `MyIndustry.Container/ProductionAllowedHosts.{Api,Identity,Gateway}.json` — ilgili projeye `appsettings.Production.json` olarak kopyalayın ve domain'leri güncelleyin.

**Redis (Production zorunlu):** Api JWT blacklist için Redis gerektirir. Dokploy app stack env'de `REDIS_HOST` ve `REDIS_PASSWORD` tanımlayın (built-in Redis panelinden). Deploy sonrası doğrulama:

```bash
docker exec myindustry-api printenv | grep -iE 'REDIS|ConnectionStrings__Redis'
docker logs myindustry-api --tail 20
```

Beklenen: `ConnectionStrings__Redis=hostname:6379,password=...` veya en azından `REDIS_HOST` + `REDIS_PASSWORD`. Api ayrıca `REDIS_HOST`/`REDIS_PASSWORD` ile connection string oluşturabilir (compose substitution başarısız olsa bile).

**Not:** Main API ve Identity API production'da yalnızca internal network / Gateway üzerinden erişilebilir olmalıdır. Local `compose.yaml` bunu varsayılan olarak uygular; debug için `--profile debug` kullanın.

Local (varsayılan — API/Identity internal):

```bash
docker compose up -d
```

Local debug (doğrudan API/Identity portları):

```bash
docker compose --profile debug up -d
```

Dokploy production: `docker-compose.dokploy.yaml` (API/Identity portları yok). Ayrıntılar: [`COMPOSE.md`](COMPOSE.md).
