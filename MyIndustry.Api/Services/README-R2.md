# Cloudflare R2 ile Görsel Yükleme

İlan görselleri S3 uyumlu Cloudflare R2 bucket'a yüklenir. R2 yapılandırılmazsa görseller yerel `wwwroot/uploads` klasörüne kaydedilir.

## R2 Kurulumu

1. **Cloudflare Dashboard** → R2 → Create bucket (örn. `myindustry-uploads`).

2. **Public erişim** (görsellerin tarayıcıda açılması için):
   - Bucket → Settings → Public access: **Allow Access** ile R2.dev subdomain açın veya **Custom Domain** ekleyin.
   - R2.dev kullanıyorsanız size verilen URL’yi (örn. `https://pub-xxxx.r2.dev`) `PublicBaseUrl` olarak kullanın.

3. **API Token**: R2 → Manage R2 API Tokens → Create API token.
   - Object Read & Write izni verin.
   - **Access Key ID** ve **Secret Access Key**’i kopyalayın.

4. **Account ID**: Cloudflare Dashboard sağ sidebar’da “Account ID” (veya Workers/R2 sayfasındaki URL’deki ID).

## appsettings

`appsettings.json` veya ortama özel `appsettings.Production.json` içinde:

```json
{
  "R2": {
    "AccountId": "CLOUDFLARE_ACCOUNT_ID",
    "AccessKeyId": "R2_ACCESS_KEY_ID",
    "SecretAccessKey": "R2_SECRET_ACCESS_KEY",
    "BucketName": "myindustry-uploads",
    "PublicBaseUrl": "https://pub-xxxx.r2.dev"
  }
}
```

- **PublicBaseUrl**: R2.dev subdomain URL’si veya custom domain (örn. `https://cdn.myindustry.com`). Sonunda `/` olmamalı.
- Gizlilik için gerçek anahtarları **environment variables** veya **User Secrets** ile verebilirsiniz; örn. `R2:SecretAccessKey` için env `R2__SecretAccessKey`.

## Davranış

- Tüm dört alan (`AccountId`, `AccessKeyId`, `SecretAccessKey`, `PublicBaseUrl`) doluysa **R2** kullanılır.
- Biri bile boşsa görseller **yerel diske** (`wwwroot/uploads`) yazılır ve sunucu URL’si döner.
