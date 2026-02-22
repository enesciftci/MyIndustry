using System.Text.Json;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.DbContext;

namespace MyIndustry.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(MyIndustryDbContext context)
    {
        // Seed locations first (if not exists)
        await SeedLocationsAsync(context);
        
        // Eğer zaten veri varsa sadece kategori ve servis düzeltmelerini yap
        if (context.Categories.Any())
        {
            Console.WriteLine("Database already has data. Checking and fixing data...");
            await FixServiceCategories(context);
            await FixServiceImages(context);
            return;
        }

        Console.WriteLine("Seeding database with dummy data...");

        // 1. Ana Kategoriler (Level 1)
        var categories = CreateMainCategories();
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {categories.Count} main categories.");

        // 2. Alt Kategoriler (Level 2)
        var subCategories = CreateSubCategories(categories);
        await context.Categories.AddRangeAsync(subCategories);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {subCategories.Count} sub-categories.");

        // 3. Derin Kategoriler (Level 3-5)
        var deepCategories = CreateDeepCategories(subCategories);
        await context.Categories.AddRangeAsync(deepCategories);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {deepCategories.Count} deep categories.");

        // 4. Subscription Plans
        var plans = CreateSubscriptionPlans();
        await context.SubscriptionPlans.AddRangeAsync(plans);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {plans.Count} subscription plans.");

        // 5. Sellers
        var sellers = CreateSellers();
        await context.Sellers.AddRangeAsync(sellers);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {sellers.Count} sellers.");

        // 6. Seller Infos
        var sellerInfos = CreateSellerInfos(sellers);
        await context.SellerInfos.AddRangeAsync(sellerInfos);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {sellerInfos.Count} seller infos.");

        // 7. Services - Tüm kategorileri al ve servisleri oluştur
        var allCategories = context.Categories.ToList();
        var services = CreateServices(sellers, allCategories);
        await context.Services.AddRangeAsync(services);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {services.Count} services.");

        Console.WriteLine("Database seeding completed!");
    }

    private static List<Category> CreateMainCategories()
    {
        return new List<Category>
        {
            new Category { Id = Guid.NewGuid(), Name = "Yedek Parça", Description = "Endüstriyel yedek parçalar", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Hidrolik Sistemler", Description = "Hidrolik pompalar ve sistemler", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Elektrik ve Otomasyon", Description = "Elektrik ve otomasyon sistemleri", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "CNC ve Talaşlı İmalat", Description = "CNC işleme hizmetleri", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Kaynak ve Metal İşleme", Description = "Kaynak ve metal işleme", IsActive = true, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Rulman ve Transmisyon", Description = "Rulmanlar ve transmisyon", IsActive = true, CreatedDate = DateTime.UtcNow },
        };
    }

    private static List<Category> CreateSubCategories(List<Category> mainCategories)
    {
        var subCategories = new List<Category>();
        
        var yedekParca = mainCategories.First(c => c.Name == "Yedek Parça");
        var hidrolik = mainCategories.First(c => c.Name == "Hidrolik Sistemler");
        var elektrik = mainCategories.First(c => c.Name == "Elektrik ve Otomasyon");
        var cnc = mainCategories.First(c => c.Name == "CNC ve Talaşlı İmalat");
        var kaynak = mainCategories.First(c => c.Name == "Kaynak ve Metal İşleme");
        var rulman = mainCategories.First(c => c.Name == "Rulman ve Transmisyon");

        // Yedek Parça altı
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Motor Yedek Parçaları", ParentId = yedekParca.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pompa Yedek Parçaları", ParentId = yedekParca.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // Hidrolik altı
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hidrolik Pompalar", ParentId = hidrolik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hidrolik Silindirler", ParentId = hidrolik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hidrolik Valfler", ParentId = hidrolik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // Elektrik altı
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "PLC ve Kontrol", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Sensörler", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Motor Sürücüler", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // CNC altı
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "CNC Torna", ParentId = cnc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "CNC Freze", ParentId = cnc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Taşlama", ParentId = cnc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // Kaynak altı
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Lazer Kesim", ParentId = kaynak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Argon Kaynak", ParentId = kaynak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        return subCategories;
    }

    private static List<Category> CreateDeepCategories(List<Category> level2Categories)
    {
        var deepCategories = new List<Category>();

        // PLC ve Kontrol > Markalar
        var plc = level2Categories.FirstOrDefault(c => c.Name == "PLC ve Kontrol");
        if (plc != null)
        {
            var siemensId = Guid.NewGuid();
            var omronId = Guid.NewGuid();
            deepCategories.Add(new Category { Id = siemensId, Name = "Siemens", ParentId = plc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = omronId, Name = "Omron", ParentId = plc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Allen Bradley", ParentId = plc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

            // Siemens > Modeller
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "S7-1200", ParentId = siemensId, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "S7-1500", ParentId = siemensId, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // Hidrolik Pompalar > Tipler
        var hidrolikPompa = level2Categories.FirstOrDefault(c => c.Name == "Hidrolik Pompalar");
        if (hidrolikPompa != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Dişli Pompalar", ParentId = hidrolikPompa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pistonlu Pompalar", ParentId = hidrolikPompa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // Motor Yedek Parçaları > Araç Tipleri
        var motorYedek = level2Categories.FirstOrDefault(c => c.Name == "Motor Yedek Parçaları");
        if (motorYedek != null)
        {
            var cekiciId = Guid.NewGuid();
            deepCategories.Add(new Category { Id = cekiciId, Name = "Çekici", ParentId = motorYedek.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kamyon", ParentId = motorYedek.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

            // Çekici > Markalar
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Scania", ParentId = cekiciId, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Volvo", ParentId = cekiciId, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Mercedes", ParentId = cekiciId, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        return deepCategories;
    }

    private static async Task FixServiceImages(MyIndustryDbContext context)
    {
        var services = context.Services.ToList();
        var fixedCount = 0;
        var random = new Random(42);

        foreach (var service in services)
        {
            if (string.IsNullOrWhiteSpace(service.ImageUrls) || service.ImageUrls == "[]" || service.ImageUrls == "null")
            {
                var images = GetImagesForTitle(service.Title ?? "", random);
                service.ImageUrls = images;
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Fixed images for {fixedCount} services.");
        }
    }

    private static async Task FixServiceCategories(MyIndustryDbContext context)
    {
        var categories = context.Categories.ToList();
        var services = context.Services.ToList();
        var fixedCount = 0;

        foreach (var service in services)
        {
            var targetCategory = FindBestCategory(service.Title ?? "", categories);
            if (targetCategory != null && service.CategoryId != targetCategory.Id)
            {
                Console.WriteLine($"Fixing category: '{service.Title}' -> {targetCategory.Name}");
                service.CategoryId = targetCategory.Id;
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Fixed {fixedCount} service categories.");
        }
    }

    private static Category? FindBestCategory(string title, List<Category> categories)
    {
        var titleLower = title.ToLower();

        // PLC/Otomasyon ürünleri -> Elektrik ve Otomasyon
        if (titleLower.Contains("plc") || titleLower.Contains("siemens") || titleLower.Contains("omron") ||
            titleLower.Contains("fotosel") || titleLower.Contains("sensör") || titleLower.Contains("invertör") ||
            titleLower.Contains("pano"))
        {
            return categories.FirstOrDefault(c => c.Name == "PLC ve Kontrol")
                ?? categories.FirstOrDefault(c => c.Name == "Elektrik ve Otomasyon");
        }

        // Hidrolik ürünleri -> Hidrolik Sistemler
        if (titleLower.Contains("hidrolik") || titleLower.Contains("pompa") || titleLower.Contains("silindir") ||
            titleLower.Contains("valf") || titleLower.Contains("tank") || titleLower.Contains("hortum"))
        {
            return categories.FirstOrDefault(c => c.Name == "Hidrolik Pompalar")
                ?? categories.FirstOrDefault(c => c.Name == "Hidrolik Sistemler");
        }

        // CNC/Talaşlı imalat
        if (titleLower.Contains("cnc") || titleLower.Contains("torna") || titleLower.Contains("freze") ||
            titleLower.Contains("taşlama"))
        {
            return categories.FirstOrDefault(c => c.Name == "CNC Torna")
                ?? categories.FirstOrDefault(c => c.Name == "CNC ve Talaşlı İmalat");
        }

        // Rulman
        if (titleLower.Contains("rulman") || titleLower.Contains("skf") || titleLower.Contains("fag") ||
            titleLower.Contains("ina"))
        {
            return categories.FirstOrDefault(c => c.Name == "Rulman ve Transmisyon");
        }

        // Metal işleme / Kaynak
        if (titleLower.Contains("kaynak") || titleLower.Contains("lazer") || titleLower.Contains("kesim") ||
            titleLower.Contains("bükme") || titleLower.Contains("konstrüksiyon"))
        {
            return categories.FirstOrDefault(c => c.Name == "Lazer Kesim")
                ?? categories.FirstOrDefault(c => c.Name == "Kaynak ve Metal İşleme");
        }

        // Yedek parça / Genel
        if (titleLower.Contains("redüktör") || titleLower.Contains("kompresör") || titleLower.Contains("konveyör") ||
            titleLower.Contains("zincir"))
        {
            return categories.FirstOrDefault(c => c.Name == "Motor Yedek Parçaları")
                ?? categories.FirstOrDefault(c => c.Name == "Yedek Parça");
        }

        return null;
    }

    private static string GetImagesForTitle(string title, Random random)
    {
        var titleLower = title.ToLower();
        string[] images;

        if (titleLower.Contains("hidrolik") || titleLower.Contains("pompa") || titleLower.Contains("silindir"))
            images = new[] { "https://picsum.photos/seed/hydraulic1/800/600", "https://picsum.photos/seed/pump1/800/600" };
        else if (titleLower.Contains("cnc") || titleLower.Contains("torna") || titleLower.Contains("freze"))
            images = new[] { "https://picsum.photos/seed/cnc1/800/600", "https://picsum.photos/seed/machine1/800/600" };
        else if (titleLower.Contains("rulman") || titleLower.Contains("skf"))
            images = new[] { "https://picsum.photos/seed/bearing1/800/600", "https://picsum.photos/seed/gear1/800/600" };
        else if (titleLower.Contains("plc") || titleLower.Contains("siemens") || titleLower.Contains("pano"))
            images = new[] { "https://picsum.photos/seed/plc1/800/600", "https://picsum.photos/seed/circuit1/800/600" };
        else if (titleLower.Contains("kaynak") || titleLower.Contains("lazer"))
            images = new[] { "https://picsum.photos/seed/welding1/800/600", "https://picsum.photos/seed/metal1/800/600" };
        else
            images = new[] { "https://picsum.photos/seed/parts1/800/600", "https://picsum.photos/seed/industrial1/800/600" };

        var count = random.Next(1, 3);
        var selectedImages = images.Take(count).ToArray();
        return "[\"" + string.Join("\",\"", selectedImages) + "\"]";
    }

    private static List<SubscriptionPlan> CreateSubscriptionPlans()
    {
        return new List<SubscriptionPlan>
        {
            new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Ücretsiz", Description = "Başlangıç paketi", SubscriptionType = SubscriptionType.Free, MonthlyPrice = 0, MonthlyPostLimit = 3, PostDurationInDays = 30, FeaturedPostLimit = 0, IsActive = true, CreatedDate = DateTime.UtcNow },
            new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Standart", Description = "Küçük işletmeler için", SubscriptionType = SubscriptionType.Standard, MonthlyPrice = 29900, MonthlyPostLimit = 15, PostDurationInDays = 45, FeaturedPostLimit = 2, IsActive = true, CreatedDate = DateTime.UtcNow },
            new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Premium", Description = "Orta ölçekli işletmeler için", SubscriptionType = SubscriptionType.Premium, MonthlyPrice = 59900, MonthlyPostLimit = 50, PostDurationInDays = 60, FeaturedPostLimit = 10, IsActive = true, CreatedDate = DateTime.UtcNow },
            new SubscriptionPlan { Id = Guid.NewGuid(), Name = "Kurumsal", Description = "Büyük firmalar için", SubscriptionType = SubscriptionType.Corporate, MonthlyPrice = 149900, MonthlyPostLimit = 999, PostDurationInDays = 90, FeaturedPostLimit = 50, IsActive = true, CreatedDate = DateTime.UtcNow }
        };
    }

    private static List<Seller> CreateSellers()
    {
        return new List<Seller>
        {
            new Seller { Id = Guid.NewGuid(), IdentityNumber = "1234567890", Title = "Anadolu Hidrolik", Description = "25 yıllık tecrübe ile hidrolik sistemler.", Sector = SellerSector.IronMongery, IsActive = true, CreatedDate = DateTime.UtcNow },
            new Seller { Id = Guid.NewGuid(), IdentityNumber = "2345678901", Title = "Marmara CNC", Description = "Modern CNC tezgahlarla hassas işçilik.", Sector = SellerSector.IronMongery, IsActive = true, CreatedDate = DateTime.UtcNow },
            new Seller { Id = Guid.NewGuid(), IdentityNumber = "3456789012", Title = "Ege Rulman", Description = "Türkiye'nin en geniş rulman stoku.", Sector = SellerSector.IronMongery, IsActive = true, CreatedDate = DateTime.UtcNow },
            new Seller { Id = Guid.NewGuid(), IdentityNumber = "4567890123", Title = "Akdeniz Otomasyon", Description = "Endüstriyel otomasyon sistemleri.", Sector = SellerSector.IronMongery, IsActive = true, CreatedDate = DateTime.UtcNow },
            new Seller { Id = Guid.NewGuid(), IdentityNumber = "5678901234", Title = "Karadeniz Metal", Description = "Metal işleme ve kaynak hizmetleri.", Sector = SellerSector.Dumper, IsActive = true, CreatedDate = DateTime.UtcNow },
        };
    }

    private static List<SellerInfo> CreateSellerInfos(List<Seller> sellers)
    {
        var infos = new List<SellerInfo>();
        var domains = new[] { "anadoluhidrolik", "marmaracnc", "egerulman", "akdenizotomasyon", "karadenizemetal" };

        for (int i = 0; i < sellers.Count; i++)
        {
            infos.Add(new SellerInfo
            {
                Id = Guid.NewGuid(),
                SellerId = sellers[i].Id,
                PhoneNumber = $"0532 {100 + i:000} {20 + i:00} {30 + i:00}",
                Email = $"info@{domains[i]}.com",
                WebSiteUrl = $"https://www.{domains[i]}.com",
                LogoUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(sellers[i].Title)}&background=random&size=200",
                CreatedDate = DateTime.UtcNow
            });
        }
        return infos;
    }

    private static List<Service> CreateServices(List<Seller> sellers, List<Category> categories)
    {
        var services = new List<Service>();
        var random = new Random(42);

        var serviceTemplates = new (string title, string desc, int price, int days, string categoryHint)[]
        {
            // Hidrolik - Hidrolik Sistemler kategorisine
            ("Hidrolik Pompa - Dişli Tip 50 Lt/dk", "Yüksek basınç dişli pompa, 250 bar", 450000, 7, "Hidrolik"),
            ("Hidrolik Silindir Ø80x500 mm", "Çift etkili hidrolik silindir", 320000, 10, "Hidrolik"),
            ("Hidrolik Yön Kontrol Valfi", "4/3 yön kontrol valfi, 24V DC", 180000, 5, "Hidrolik"),
            ("Hidrolik Tank 100 Lt", "Çelik tank, filtre dahil", 250000, 14, "Hidrolik"),

            // CNC - CNC ve Talaşlı İmalat kategorisine
            ("CNC Torna İşleme Hizmeti", "Hassas CNC torna, ±0.01 tolerans", 15000, 5, "CNC"),
            ("CNC Freze İşleme", "3 ve 5 eksen CNC freze", 25000, 7, "CNC"),
            ("Hassas Taşlama Hizmeti", "Ra 0.4 yüzey kalitesi", 20000, 4, "CNC"),

            // Rulman - Rulman ve Transmisyon kategorisine
            ("SKF 6205-2RS Rulman", "Orijinal SKF, çift kauçuk conta", 8500, 2, "Rulman"),
            ("FAG 32210 Konik Rulman", "Konik makaralı rulman", 24000, 3, "Rulman"),
            ("Rulman Montaj Seti", "Isıtıcı ve montaj aletleri", 350000, 7, "Rulman"),

            // Elektrik/Otomasyon - Elektrik ve Otomasyon kategorisine
            ("Siemens S7-1200 PLC", "CPU 1214C DC/DC/DC, Ethernet", 850000, 5, "PLC"),
            ("Omron Fotosel Sensör", "E3Z serisi, 4m mesafe", 32000, 3, "PLC"),
            ("Delta VFD Frekans İnvertör", "7.5kW, 380V 3 faz", 420000, 7, "PLC"),
            ("Endüstriyel Pano İmalatı", "Özel tasarım elektrik panosu", 1500000, 21, "PLC"),

            // Metal İşleme - Kaynak ve Metal İşleme kategorisine
            ("Çelik Konstrüksiyon İmalatı", "Teknik projeye göre üretim", 5000000, 30, "Kaynak"),
            ("Lazer Kesim Hizmeti", "CNC lazer, 20mm'e kadar", 10000, 3, "Kaynak"),
            ("Argon Kaynak Hizmeti", "TIG/MIG kaynak", 25000, 5, "Kaynak"),

            // Yedek Parça - Yedek Parça kategorisine
            ("Redüktör Yedek Parça Seti", "SEW R47 serisi tamir seti", 180000, 10, "Yedek"),
            ("Kompresör Tamir Kiti", "Atlas Copco GA bakım seti", 95000, 7, "Yedek"),
            ("Konveyör Bant Tamburu", "Ø400x800 mm, kauçuk kaplı", 280000, 14, "Yedek"),
        };

        // Default category if no match found
        var defaultCategory = categories.First(c => c.ParentId == null);
        
        foreach (var (title, desc, price, days, categoryHint) in serviceTemplates)
        {
            var seller = sellers[random.Next(sellers.Count)];
            var category = FindCategoryByHint(categoryHint, categories) ?? defaultCategory;

            services.Add(new Service
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = desc,
                Price = price,
                EstimatedEndDay = days,
                SellerId = seller.Id,
                CategoryId = category.Id,
                IsActive = true,
                IsApproved = true,
                ViewCount = random.Next(10, 500),
                ImageUrls = GetImagesForTitle(title, random),
                CreatedDate = DateTime.UtcNow.AddDays(-random.Next(1, 60))
            });
        }

        return services;
    }

    private static Category? FindCategoryByHint(string hint, List<Category> categories)
    {
        return hint switch
        {
            "Hidrolik" => categories.FirstOrDefault(c => c.Name == "Hidrolik Pompalar") ?? categories.FirstOrDefault(c => c.Name == "Hidrolik Sistemler"),
            "CNC" => categories.FirstOrDefault(c => c.Name == "CNC Torna") ?? categories.FirstOrDefault(c => c.Name == "CNC ve Talaşlı İmalat"),
            "Rulman" => categories.FirstOrDefault(c => c.Name == "Rulman ve Transmisyon"),
            "PLC" => categories.FirstOrDefault(c => c.Name == "PLC ve Kontrol") ?? categories.FirstOrDefault(c => c.Name == "Elektrik ve Otomasyon"),
            "Kaynak" => categories.FirstOrDefault(c => c.Name == "Lazer Kesim") ?? categories.FirstOrDefault(c => c.Name == "Kaynak ve Metal İşleme"),
            "Yedek" => categories.FirstOrDefault(c => c.Name == "Motor Yedek Parçaları") ?? categories.FirstOrDefault(c => c.Name == "Yedek Parça"),
            _ => categories.FirstOrDefault(c => c.ParentId == null)
        };
    }
    
    /// <summary>
    /// Lokasyon verilerini (İl, İlçe, Mahalle) seed eder
    /// </summary>
    private static async Task SeedLocationsAsync(MyIndustryDbContext context)
    {
        // Zaten lokasyon verisi varsa atla
        if (context.Cities.Any())
        {
            Console.WriteLine("Location data already exists. Skipping location seeding.");
            return;
        }

        Console.WriteLine("Seeding location data (Cities, Districts, Neighborhoods)...");

        try
        {
            // JSON dosyasını oku
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "locations.json");
            
            // Docker container'da farklı path olabilir
            if (!File.Exists(jsonPath))
            {
                jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "locations.json");
            }
            
            if (!File.Exists(jsonPath))
            {
                Console.WriteLine($"Location data file not found at: {jsonPath}");
                Console.WriteLine("Skipping location seeding.");
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var locationData = JsonSerializer.Deserialize<LocationData>(jsonContent, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (locationData == null)
            {
                Console.WriteLine("Failed to parse location data.");
                return;
            }

            // Cities
            var cities = locationData.Cities.Select(c => new City
            {
                Id = Guid.Parse(c.Id),
                Name = c.Name,
                PlateCode = c.PlateCode,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }).ToList();

            await context.Cities.AddRangeAsync(cities);
            await context.SaveChangesAsync();
            Console.WriteLine($"Created {cities.Count} cities.");

            // Districts - batch insert
            var batchSize = 500;
            var districtsList = locationData.Districts.Select(d => new District
            {
                Id = Guid.Parse(d.Id),
                Name = d.Name,
                CityId = Guid.Parse(d.CityId),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }).ToList();

            for (int i = 0; i < districtsList.Count; i += batchSize)
            {
                var batch = districtsList.Skip(i).Take(batchSize);
                await context.Districts.AddRangeAsync(batch);
                await context.SaveChangesAsync();
            }
            Console.WriteLine($"Created {districtsList.Count} districts.");

            // Neighborhoods - batch insert (large dataset)
            var neighborhoodsList = locationData.Neighborhoods.Select(n => new Neighborhood
            {
                Id = Guid.Parse(n.Id),
                Name = n.Name,
                DistrictId = Guid.Parse(n.DistrictId),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }).ToList();

            for (int i = 0; i < neighborhoodsList.Count; i += batchSize)
            {
                var batch = neighborhoodsList.Skip(i).Take(batchSize);
                await context.Neighborhoods.AddRangeAsync(batch);
                await context.SaveChangesAsync();
                
                if ((i + batchSize) % 5000 == 0)
                {
                    Console.WriteLine($"Progress: {Math.Min(i + batchSize, neighborhoodsList.Count)}/{neighborhoodsList.Count} neighborhoods...");
                }
            }
            Console.WriteLine($"Created {neighborhoodsList.Count} neighborhoods.");

            Console.WriteLine("Location seeding completed!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding locations: {ex.Message}");
        }
    }

    // JSON deserialization classes
    private class LocationData
    {
        public List<CityDto> Cities { get; set; } = new();
        public List<DistrictDto> Districts { get; set; } = new();
        public List<NeighborhoodDto> Neighborhoods { get; set; } = new();
    }

    private class CityDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int PlateCode { get; set; }
    }

    private class DistrictDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string CityId { get; set; } = "";
    }

    private class NeighborhoodDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string DistrictId { get; set; } = "";
    }
}
