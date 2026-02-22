using System.Text.Json;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.DbContext;

namespace MyIndustry.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(MyIndustryDbContext context)
    {
        try
        {
            Console.WriteLine("=== Starting Data Seeding ===");
            Console.WriteLine($"Categories count: {context.Categories.Count()}");
            Console.WriteLine($"Cities count: {context.Cities.Count()}");
            Console.WriteLine($"Services count: {context.Services.Count()}");
            
            // Seed locations first (if not exists)
            await SeedLocationsAsync(context);
            
            // Eğer zaten veri varsa eksik kategorileri ekle ve düzeltmeleri yap
            if (context.Categories.Any())
            {
                Console.WriteLine("Database already has category data. Checking for missing categories...");
                await SeedMissingCategoriesAsync(context);
                await FixServiceCategories(context);
                await FixServiceImages(context);
                return;
            }

            Console.WriteLine("No categories found. Seeding database with dummy data...");

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

        Console.WriteLine("=== Database seeding completed! ===");
        Console.WriteLine($"Final counts - Categories: {context.Categories.Count()}, Services: {context.Services.Count()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ERROR during seeding: {ex.Message} ===");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw; // Re-throw to ensure the error is visible
        }
    }

    /// <summary>
    /// Mevcut veritabanına eksik kategorileri ekler
    /// </summary>
    private static async Task SeedMissingCategoriesAsync(MyIndustryDbContext context)
    {
        var existingCategories = context.Categories.ToList();
        var existingNames = existingCategories.Select(c => c.Name).ToHashSet();
        var addedCount = 0;

        // 1. Eksik Ana Kategorileri Ekle
        var mainCategoryNames = new[]
        {
            ("Makina", "Endüstriyel makinalar ve ekipmanlar"),
            ("Hidrolik Pnömatik", "Hidrolik ve pnömatik sistemler"),
            ("Elektrik ve Enerji", "Elektrik, otomasyon ve enerji sistemleri"),
            ("Metal", "Metal ürünler, sac, profil, boru"),
            ("Yedek Parça", "Endüstriyel yedek parçalar"),
            ("İş Makineleri", "İnşaat ve iş makineleri"),
            ("Rulman ve Transmisyon", "Rulmanlar, dişliler, kayışlar"),
            ("Ambalaj", "Ambalaj makineleri ve malzemeleri"),
            ("Hortum ve Bağlantı", "Endüstriyel hortumlar ve bağlantı elemanları"),
            ("Tarım ve Gıda", "Tarım makineleri ve gıda ekipmanları"),
            ("Yapı ve İnşaat", "Yapı malzemeleri ve inşaat ekipmanları"),
            ("Ölçü ve Kontrol", "Ölçüm aletleri ve kontrol sistemleri"),
            ("Endüstriyel Mutfak", "Endüstriyel mutfak ekipmanları"),
            ("Tekstil Makineleri", "Tekstil ve konfeksiyon makineleri"),
            ("Matbaa ve Baskı", "Matbaa ve baskı makineleri"),
            ("Plastik ve Kauçuk", "Plastik ve kauçuk işleme makineleri"),
            ("Ahşap İşleme", "Ahşap işleme makineleri")
        };

        foreach (var (name, desc) in mainCategoryNames)
        {
            if (!existingNames.Contains(name))
            {
                var category = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = desc,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                await context.Categories.AddAsync(category);
                existingNames.Add(name);
                addedCount++;
                Console.WriteLine($"Added main category: {name}");
            }
        }
        
        if (addedCount > 0)
        {
            await context.SaveChangesAsync();
        }

        // Refresh existing categories after main categories are added
        existingCategories = context.Categories.ToList();

        // 2. Alt Kategorileri Ekle
        var subCategoryDefinitions = GetSubCategoryDefinitions();
        foreach (var (parentName, subCategories) in subCategoryDefinitions)
        {
            var parent = existingCategories.FirstOrDefault(c => c.Name == parentName);
            if (parent == null)
            {
                Console.WriteLine($"Warning: Parent category not found: {parentName}");
                continue;
            }

            foreach (var subName in subCategories)
            {
                if (!existingNames.Contains(subName))
                {
                    var category = new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = subName,
                        ParentId = parent.Id,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    await context.Categories.AddAsync(category);
                    existingNames.Add(subName);
                    addedCount++;
                    Console.WriteLine($"Added sub-category: {parentName} > {subName}");
                }
            }
        }

        if (addedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Added {addedCount} missing categories.");
        }
        else
        {
            Console.WriteLine("No missing categories to add.");
        }

        // Refresh for deep categories
        existingCategories = context.Categories.ToList();

        // 3. Derin Kategorileri Ekle (Level 3+)
        var deepAddedCount = 0;
        var deepCategoryDefinitions = GetDeepCategoryDefinitions();
        foreach (var (parentName, deepCategories) in deepCategoryDefinitions)
        {
            var parent = existingCategories.FirstOrDefault(c => c.Name == parentName);
            if (parent == null)
            {
                continue;
            }

            foreach (var deepName in deepCategories)
            {
                if (!existingNames.Contains(deepName))
                {
                    var category = new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = deepName,
                        ParentId = parent.Id,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    await context.Categories.AddAsync(category);
                    existingNames.Add(deepName);
                    deepAddedCount++;
                }
            }
        }

        if (deepAddedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Added {deepAddedCount} deep categories.");
        }
    }

    private static List<(string ParentName, string[] SubCategories)> GetSubCategoryDefinitions()
    {
        return new List<(string, string[])>
        {
            ("Makina", new[] { "CNC Makinaları", "Talaşlı İmalat Makineleri", "Kompresör", "Lazer Kesim Makinası", 
                "Pompa Modelleri", "Kaynak Makineleri", "Pres Makineleri", "Sac İşleme Makinaları", 
                "Enjeksiyon Makinaları", "Kumlama Makineleri", "Boru Kesme ve Diş Açma", "Temizlik Makineleri" }),
            
            ("Hidrolik Pnömatik", new[] { "Hidrolik Pompalar", "Hidrolik Silindirler", "Hidrolik Valfler", 
                "Hidrolik Motorlar", "Hidrolik Tanklar", "Pnömatik Silindirler", "Pnömatik Valfler", "Hava Hazırlama Üniteleri" }),
            
            ("Elektrik ve Enerji", new[] { "PLC ve Kontrol Sistemleri", "Sensörler", "Motor Sürücüler (Invertör)", 
                "Elektrik Motorları", "Güç Kaynakları", "Enerji Kabloları", "Pano ve Pano Malzemeleri", 
                "Aydınlatma", "Jeneratör", "UPS ve Kesintisiz Güç" }),
            
            ("Metal", new[] { "Sac ve Levha", "Profil ve Kare Boru", "Çelik Boru", "Paslanmaz Çelik", 
                "Alüminyum", "Bakır ve Pirinç", "Döküm ve Dövme" }),
            
            ("Yedek Parça", new[] { "Motor Yedek Parçaları", "Pompa Yedek Parçaları", "Kompresör Yedek Parçaları", 
                "Redüktör Yedek Parçaları", "Filtreler", "Contalar ve Sızdırmazlık" }),
            
            ("İş Makineleri", new[] { "Forklift", "Vinç ve Kaldırma", "Ekskavatör", "Yükleyici", "Transpalet", "Konveyör Sistemleri" }),
            
            ("Rulman ve Transmisyon", new[] { "Bilyalı Rulmanlar", "Makaralı Rulmanlar", "Konik Rulmanlar", 
                "Kayışlar", "Zincirler", "Dişliler", "Redüktörler", "Kaplinler" }),
            
            ("Ambalaj", new[] { "Paketleme Makineleri", "Etiketleme Makineleri", "Streç Sarma", "Ambalaj Malzemeleri" }),
            
            ("Hortum ve Bağlantı", new[] { "Hidrolik Hortum", "Pnömatik Hortum", "PVC Hortum", "Rekorlar ve Nipeller", "Bağlantı Elemanları" }),
            
            ("Tarım ve Gıda", new[] { "Tarım Makineleri", "Sulama Sistemleri", "Gıda İşleme Makineleri", "Soğutma Sistemleri" }),
            
            ("Yapı ve İnşaat", new[] { "İnşaat Makineleri", "İskele ve Kalıp", "Beton Ekipmanları", "Yapı Malzemeleri" }),
            
            ("Ölçü ve Kontrol", new[] { "Ölçüm Aletleri", "Kalibrasyon Cihazları", "Test ve Muayene", "Tartı Sistemleri" }),
            
            // Sahibinden.com kategorileri
            ("Endüstriyel Mutfak", new[] { "Fırınlar", "Bulaşık Makineleri", "Soğutucular ve Buzluklar", "Kızartma Makineleri", 
                "Izgara ve Tost Makineleri", "Çay ve Kahve Makineleri", "Et İşleme Makineleri", "Hamur İşleme Makineleri", 
                "Servis Ekipmanları", "Bain-Marie ve Reşolar" }),
            
            ("Tekstil Makineleri", new[] { "Dokuma Makineleri", "Örme Makineleri", "Dikiş Makineleri", "Kesim Makineleri",
                "Nakış Makineleri", "Baskı Makineleri", "Ütü ve Pres Makineleri", "Yıkama Makineleri" }),
            
            ("Matbaa ve Baskı", new[] { "Ofset Baskı Makineleri", "Dijital Baskı Makineleri", "Flekso Baskı Makineleri",
                "Serigrafi Makineleri", "Cilt Makineleri", "Kesim ve Katlama Makineleri", "Laminasyon Makineleri" }),
            
            ("Plastik ve Kauçuk", new[] { "Plastik Enjeksiyon Makineleri", "Şişirme Makineleri", "Ekstrüzyon Makineleri",
                "Termoform Makineleri", "Kauçuk Presleri", "Granül Makineleri", "Karıştırıcı ve Kırıcılar" }),
            
            ("Ahşap İşleme", new[] { "Ahşap Torna", "Ahşap Freze", "Planya Makineleri", "Zımpara Makineleri",
                "Panel Ebatlama", "Kenar Bantlama", "CNC Router", "Testere Makineleri" })
        };
    }

    private static List<(string ParentName, string[] DeepCategories)> GetDeepCategoryDefinitions()
    {
        return new List<(string, string[])>
        {
            // CNC Makinaları
            ("CNC Makinaları", new[] { "CNC Torna Tezgahları", "CNC İşleme Merkezi", "CNC Freze", "CNC Router", 
                "CNC Plazma Kesim", "CNC Taşlama", "CNC Boru Bükme", "CNC Aksam ve Parça" }),
            
            // Talaşlı İmalat
            ("Talaşlı İmalat Makineleri", new[] { "Torna Tezgahları", "Freze Tezgahları", "Matkap Tezgahları", 
                "Taşlama Tezgahları", "Planya Tezgahları", "Testere Makineleri" }),
            
            // Kompresör
            ("Kompresör", new[] { "Vidalı Kompresör", "Pistonlu Kompresör", "Sessiz Kompresör", 
                "Yağsız Kompresör", "Kompresör Yedek Parça", "Hava Kurutucu" }),
            
            // Pompa Modelleri
            ("Pompa Modelleri", new[] { "Hidrolik Pompa", "Santrifüj Pompa", "Dalgıç Pompa", 
                "Dişli Pompa", "Pistonlu Pompa", "Dozaj Pompası", "Vakum Pompası" }),
            
            // Kaynak Makineleri
            ("Kaynak Makineleri", new[] { "MIG/MAG Kaynak", "TIG (Argon) Kaynak", "Elektrot Kaynak", 
                "Punta Kaynak", "Plazma Kesim", "Kaynak Ekipmanları" }),
            
            // Pres Makineleri
            ("Pres Makineleri", new[] { "Hidrolik Pres", "Eksantrik Pres", "Abkant Pres", 
                "Derin Çekme Pres", "Atölye Tipi Pres" }),
            
            // PLC Markaları
            ("PLC ve Kontrol Sistemleri", new[] { "Siemens", "Omron", "Mitsubishi", "Allen Bradley", 
                "Schneider", "ABB", "Delta" }),
            
            // Sensör Tipleri
            ("Sensörler", new[] { "Fotosel Sensörler", "Endüktif Sensörler", "Kapasitif Sensörler", 
                "Basınç Sensörleri", "Sıcaklık Sensörleri", "Ultrasonik Sensörler", "Encoder" }),
            
            // Motor Sürücü Markaları
            ("Motor Sürücüler (Invertör)", new[] { "Siemens", "ABB", "Schneider", "Delta", 
                "Danfoss", "Yaskawa", "Mitsubishi" }),
            
            // Hidrolik Pompa Tipleri
            ("Hidrolik Pompalar", new[] { "Dişli Pompalar", "Paletli Pompalar", "Pistonlu Pompalar", "El Pompaları" }),
            
            // Rulman Markaları
            ("Bilyalı Rulmanlar", new[] { "SKF", "FAG", "NSK", "NTN", "INA", "Koyo", "Timken" }),
            
            // Kayış Tipleri ve Markaları
            ("Kayışlar", new[] { "V Kayışlar", "Düz Kayışlar", "Trapezoidal Kayışlar", 
                "Zamanlama Kayışları", "Gates", "Optibelt", "Continental" }),
            
            // Redüktör Markaları
            ("Redüktörler", new[] { "SEW", "Nord", "Bonfiglioli", "Rossi", "Siti", "Yılmaz Redüktör" }),
            
            // İş Makineleri Alt Kategorileri (Sahibinden.com tarzı)
            ("Forklift", new[] { "Dizel Forklift", "Elektrikli Forklift", "LPG Forklift", "Reach Truck", "Stacker", "Sipariş Toplama" }),
            
            ("Vinç ve Kaldırma", new[] { "Mobil Vinç", "Kule Vinç", "Köprü Vinç", "Portal Vinç", "Caraskal", "Platform", "Ceraskal" }),
            
            ("Ekskavatör", new[] { "Paletli Ekskavatör", "Lastik Tekerlekli Ekskavatör", "Mini Ekskavatör", "Uzun Bom Ekskavatör" }),
            
            ("Yükleyici", new[] { "Beko Loder", "Lastik Tekerlekli Yükleyici", "Mini Yükleyici", "Skid Steer", "Teleskopik Yükleyici" }),
            
            // Tarım Makineleri Alt Kategorileri
            ("Tarım Makineleri", new[] { "Traktör", "Biçerdöver", "Pulluk", "Ekim Makinesi", "Gübre Makinesi", "İlaçlama Makinesi",
                "Römork", "Balya Makinesi", "Sulama Sistemleri" }),
            
            // Jeneratör Markaları
            ("Jeneratör", new[] { "Dizel Jeneratör", "Benzinli Jeneratör", "Doğalgaz Jeneratör", "Portatif Jeneratör",
                "Caterpillar", "Cummins", "Perkins", "FG Wilson", "Aksa", "Teksan" }),
            
            // Kompresör Markaları
            ("Kompresör", new[] { "Atlas Copco", "Kaeser", "Ingersoll Rand", "Gardner Denver", "Boge", "Fini" })
        };
    }

    private static List<Category> CreateMainCategories()
    {
        // Sanayiden.com ve sektör standardına göre ana kategoriler
        return new List<Category>
        {
            // Makina Ana Kategorisi
            new Category { Id = Guid.NewGuid(), Name = "Makina", Description = "Endüstriyel makinalar ve ekipmanlar", IsActive = true, CreatedDate = DateTime.UtcNow },
            // Hidrolik Pnömatik
            new Category { Id = Guid.NewGuid(), Name = "Hidrolik Pnömatik", Description = "Hidrolik ve pnömatik sistemler", IsActive = true, CreatedDate = DateTime.UtcNow },
            // Elektrik & Enerji
            new Category { Id = Guid.NewGuid(), Name = "Elektrik ve Enerji", Description = "Elektrik, otomasyon ve enerji sistemleri", IsActive = true, CreatedDate = DateTime.UtcNow },
            // Metal
            new Category { Id = Guid.NewGuid(), Name = "Metal", Description = "Metal ürünler, sac, profil, boru", IsActive = true, CreatedDate = DateTime.UtcNow },
            // Yedek Parça
            new Category { Id = Guid.NewGuid(), Name = "Yedek Parça", Description = "Endüstriyel yedek parçalar", IsActive = true, CreatedDate = DateTime.UtcNow },
            // İş Makineleri
            new Category { Id = Guid.NewGuid(), Name = "İş Makineleri", Description = "İnşaat ve iş makineleri", IsActive = true, CreatedDate = DateTime.UtcNow },
            // Rulman ve Transmisyon
            new Category { Id = Guid.NewGuid(), Name = "Rulman ve Transmisyon", Description = "Rulmanlar, dişliler, kayışlar", IsActive = true, CreatedDate = DateTime.UtcNow },
            // Ambalaj
            new Category { Id = Guid.NewGuid(), Name = "Ambalaj", Description = "Ambalaj makineleri ve malzemeleri", IsActive = true, CreatedDate = DateTime.UtcNow },
            // Hortum ve Bağlantı
            new Category { Id = Guid.NewGuid(), Name = "Hortum ve Bağlantı", Description = "Endüstriyel hortumlar ve bağlantı elemanları", IsActive = true, CreatedDate = DateTime.UtcNow },
            // Tarım ve Gıda
            new Category { Id = Guid.NewGuid(), Name = "Tarım ve Gıda", Description = "Tarım makineleri ve gıda ekipmanları", IsActive = true, CreatedDate = DateTime.UtcNow },
            // Yapı ve İnşaat
            new Category { Id = Guid.NewGuid(), Name = "Yapı ve İnşaat", Description = "Yapı malzemeleri ve inşaat ekipmanları", IsActive = true, CreatedDate = DateTime.UtcNow },
            // Ölçü ve Kontrol
            new Category { Id = Guid.NewGuid(), Name = "Ölçü ve Kontrol", Description = "Ölçüm aletleri ve kontrol sistemleri", IsActive = true, CreatedDate = DateTime.UtcNow },
        };
    }

    private static List<Category> CreateSubCategories(List<Category> mainCategories)
    {
        var subCategories = new List<Category>();
        
        var makina = mainCategories.First(c => c.Name == "Makina");
        var hidrolik = mainCategories.First(c => c.Name == "Hidrolik Pnömatik");
        var elektrik = mainCategories.First(c => c.Name == "Elektrik ve Enerji");
        var metal = mainCategories.First(c => c.Name == "Metal");
        var yedekParca = mainCategories.First(c => c.Name == "Yedek Parça");
        var isMakineleri = mainCategories.First(c => c.Name == "İş Makineleri");
        var rulman = mainCategories.First(c => c.Name == "Rulman ve Transmisyon");
        var ambalaj = mainCategories.First(c => c.Name == "Ambalaj");
        var hortum = mainCategories.First(c => c.Name == "Hortum ve Bağlantı");
        var tarim = mainCategories.First(c => c.Name == "Tarım ve Gıda");
        var yapi = mainCategories.First(c => c.Name == "Yapı ve İnşaat");
        var olcu = mainCategories.First(c => c.Name == "Ölçü ve Kontrol");
        var endustriyelMutfak = mainCategories.FirstOrDefault(c => c.Name == "Endüstriyel Mutfak");
        var tekstil = mainCategories.FirstOrDefault(c => c.Name == "Tekstil Makineleri");
        var matbaa = mainCategories.FirstOrDefault(c => c.Name == "Matbaa ve Baskı");
        var plastik = mainCategories.FirstOrDefault(c => c.Name == "Plastik ve Kauçuk");
        var ahsap = mainCategories.FirstOrDefault(c => c.Name == "Ahşap İşleme");

        // ===== MAKİNA ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "CNC Makinaları", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Talaşlı İmalat Makineleri", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kompresör", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Lazer Kesim Makinası", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pompa Modelleri", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kaynak Makineleri", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pres Makineleri", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Sac İşleme Makinaları", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Enjeksiyon Makinaları", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kumlama Makineleri", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Boru Kesme ve Diş Açma", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Temizlik Makineleri", ParentId = makina.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== HİDROLİK PNÖMATİK ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hidrolik Pompalar", ParentId = hidrolik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hidrolik Silindirler", ParentId = hidrolik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hidrolik Valfler", ParentId = hidrolik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hidrolik Motorlar", ParentId = hidrolik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hidrolik Tanklar", ParentId = hidrolik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pnömatik Silindirler", ParentId = hidrolik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pnömatik Valfler", ParentId = hidrolik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hava Hazırlama Üniteleri", ParentId = hidrolik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== ELEKTRİK VE ENERJİ ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "PLC ve Kontrol Sistemleri", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Sensörler", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Motor Sürücüler (Invertör)", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Elektrik Motorları", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Güç Kaynakları", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Enerji Kabloları", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pano ve Pano Malzemeleri", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Aydınlatma", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Jeneratör", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "UPS ve Kesintisiz Güç", ParentId = elektrik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== METAL ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Sac ve Levha", ParentId = metal.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Profil ve Kare Boru", ParentId = metal.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Çelik Boru", ParentId = metal.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Paslanmaz Çelik", ParentId = metal.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Alüminyum", ParentId = metal.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Bakır ve Pirinç", ParentId = metal.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Döküm ve Dövme", ParentId = metal.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== YEDEK PARÇA ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Motor Yedek Parçaları", ParentId = yedekParca.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pompa Yedek Parçaları", ParentId = yedekParca.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kompresör Yedek Parçaları", ParentId = yedekParca.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Redüktör Yedek Parçaları", ParentId = yedekParca.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Filtreler", ParentId = yedekParca.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Contalar ve Sızdırmazlık", ParentId = yedekParca.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== İŞ MAKİNELERİ ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Forklift", ParentId = isMakineleri.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Vinç ve Kaldırma", ParentId = isMakineleri.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Ekskavatör", ParentId = isMakineleri.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Yükleyici", ParentId = isMakineleri.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Transpalet", ParentId = isMakineleri.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Konveyör Sistemleri", ParentId = isMakineleri.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== RULMAN VE TRANSMİSYON ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Bilyalı Rulmanlar", ParentId = rulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Makaralı Rulmanlar", ParentId = rulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Konik Rulmanlar", ParentId = rulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kayışlar", ParentId = rulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Zincirler", ParentId = rulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Dişliler", ParentId = rulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Redüktörler", ParentId = rulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kaplinler", ParentId = rulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== AMBALAJ ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Paketleme Makineleri", ParentId = ambalaj.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Etiketleme Makineleri", ParentId = ambalaj.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Streç Sarma", ParentId = ambalaj.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Ambalaj Malzemeleri", ParentId = ambalaj.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== HORTUM VE BAĞLANTI ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hidrolik Hortum", ParentId = hortum.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pnömatik Hortum", ParentId = hortum.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "PVC Hortum", ParentId = hortum.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Rekorlar ve Nipeller", ParentId = hortum.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Bağlantı Elemanları", ParentId = hortum.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== TARIM VE GIDA ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Tarım Makineleri", ParentId = tarim.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Sulama Sistemleri", ParentId = tarim.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Gıda İşleme Makineleri", ParentId = tarim.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Soğutma Sistemleri", ParentId = tarim.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== YAPI VE İNŞAAT ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "İnşaat Makineleri", ParentId = yapi.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "İskele ve Kalıp", ParentId = yapi.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Beton Ekipmanları", ParentId = yapi.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Yapı Malzemeleri", ParentId = yapi.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== ÖLÇÜ VE KONTROL ALT KATEGORİLERİ =====
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Ölçüm Aletleri", ParentId = olcu.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kalibrasyon Cihazları", ParentId = olcu.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Test ve Muayene", ParentId = olcu.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Tartı Sistemleri", ParentId = olcu.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

        // ===== ENDÜSTRİYEL MUTFAK ALT KATEGORİLERİ (Sahibinden.com) =====
        if (endustriyelMutfak != null)
        {
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Fırınlar", ParentId = endustriyelMutfak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Bulaşık Makineleri", ParentId = endustriyelMutfak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Soğutucular ve Buzluklar", ParentId = endustriyelMutfak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kızartma Makineleri", ParentId = endustriyelMutfak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Izgara ve Tost Makineleri", ParentId = endustriyelMutfak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Çay ve Kahve Makineleri", ParentId = endustriyelMutfak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Et İşleme Makineleri", ParentId = endustriyelMutfak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hamur İşleme Makineleri", ParentId = endustriyelMutfak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Servis Ekipmanları", ParentId = endustriyelMutfak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Bain-Marie ve Reşolar", ParentId = endustriyelMutfak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== TEKSTİL MAKİNELERİ ALT KATEGORİLERİ =====
        if (tekstil != null)
        {
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Dokuma Makineleri", ParentId = tekstil.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Örme Makineleri", ParentId = tekstil.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Dikiş Makineleri", ParentId = tekstil.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kesim Makineleri", ParentId = tekstil.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Nakış Makineleri", ParentId = tekstil.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Tekstil Baskı Makineleri", ParentId = tekstil.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Ütü ve Pres Makineleri", ParentId = tekstil.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Yıkama Makineleri", ParentId = tekstil.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== MATBAA VE BASKI ALT KATEGORİLERİ =====
        if (matbaa != null)
        {
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Ofset Baskı Makineleri", ParentId = matbaa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Dijital Baskı Makineleri", ParentId = matbaa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Flekso Baskı Makineleri", ParentId = matbaa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Serigrafi Makineleri", ParentId = matbaa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Cilt Makineleri", ParentId = matbaa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kesim ve Katlama Makineleri", ParentId = matbaa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Laminasyon Makineleri", ParentId = matbaa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== PLASTİK VE KAUÇUK ALT KATEGORİLERİ =====
        if (plastik != null)
        {
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Plastik Enjeksiyon Makineleri", ParentId = plastik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Şişirme Makineleri", ParentId = plastik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Ekstrüzyon Makineleri", ParentId = plastik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Termoform Makineleri", ParentId = plastik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kauçuk Presleri", ParentId = plastik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Granül Makineleri", ParentId = plastik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Karıştırıcı ve Kırıcılar", ParentId = plastik.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== AHŞAP İŞLEME ALT KATEGORİLERİ =====
        if (ahsap != null)
        {
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Ahşap Torna", ParentId = ahsap.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Ahşap Freze", ParentId = ahsap.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Planya Makineleri", ParentId = ahsap.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Zımpara Makineleri", ParentId = ahsap.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Panel Ebatlama", ParentId = ahsap.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kenar Bantlama", ParentId = ahsap.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Ahşap CNC Router", ParentId = ahsap.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            subCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Ahşap Testere Makineleri", ParentId = ahsap.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        return subCategories;
    }

    private static List<Category> CreateDeepCategories(List<Category> level2Categories)
    {
        var deepCategories = new List<Category>();

        // ===== CNC MAKİNALARI ALT KATEGORİLERİ =====
        var cncMakinalar = level2Categories.FirstOrDefault(c => c.Name == "CNC Makinaları");
        if (cncMakinalar != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "CNC Torna Tezgahları", ParentId = cncMakinalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "CNC İşleme Merkezi", ParentId = cncMakinalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "CNC Freze", ParentId = cncMakinalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "CNC Router", ParentId = cncMakinalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "CNC Plazma Kesim", ParentId = cncMakinalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "CNC Taşlama", ParentId = cncMakinalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "CNC Boru Bükme", ParentId = cncMakinalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "CNC Aksam ve Parça", ParentId = cncMakinalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== TALAŞLI İMALAT MAKİNELERİ ALT KATEGORİLERİ =====
        var talasli = level2Categories.FirstOrDefault(c => c.Name == "Talaşlı İmalat Makineleri");
        if (talasli != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Torna Tezgahları", ParentId = talasli.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Freze Tezgahları", ParentId = talasli.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Matkap Tezgahları", ParentId = talasli.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Taşlama Tezgahları", ParentId = talasli.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Planya Tezgahları", ParentId = talasli.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Testere Makineleri", ParentId = talasli.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== KOMPRESÖR ALT KATEGORİLERİ =====
        var kompresor = level2Categories.FirstOrDefault(c => c.Name == "Kompresör");
        if (kompresor != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Vidalı Kompresör", ParentId = kompresor.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pistonlu Kompresör", ParentId = kompresor.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Sessiz Kompresör", ParentId = kompresor.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Yağsız Kompresör", ParentId = kompresor.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kompresör Yedek Parça", ParentId = kompresor.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hava Kurutucu", ParentId = kompresor.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== POMPA TİPLERİ =====
        var pompalar = level2Categories.FirstOrDefault(c => c.Name == "Pompa Modelleri");
        if (pompalar != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hidrolik Pompa", ParentId = pompalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Santrifüj Pompa", ParentId = pompalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Dalgıç Pompa", ParentId = pompalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Dişli Pompa", ParentId = pompalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pistonlu Pompa", ParentId = pompalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Dozaj Pompası", ParentId = pompalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Vakum Pompası", ParentId = pompalar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== KAYNAK MAKİNELERİ ALT KATEGORİLERİ =====
        var kaynak = level2Categories.FirstOrDefault(c => c.Name == "Kaynak Makineleri");
        if (kaynak != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "MIG/MAG Kaynak", ParentId = kaynak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "TIG (Argon) Kaynak", ParentId = kaynak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Elektrot Kaynak", ParentId = kaynak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Punta Kaynak", ParentId = kaynak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Plazma Kesim", ParentId = kaynak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kaynak Ekipmanları", ParentId = kaynak.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== PRES MAKİNELERİ ALT KATEGORİLERİ =====
        var pres = level2Categories.FirstOrDefault(c => c.Name == "Pres Makineleri");
        if (pres != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Hidrolik Pres", ParentId = pres.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Eksantrik Pres", ParentId = pres.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Abkant Pres", ParentId = pres.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Derin Çekme Pres", ParentId = pres.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Atölye Tipi Pres", ParentId = pres.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== PLC VE KONTROL MARKALARI =====
        var plc = level2Categories.FirstOrDefault(c => c.Name == "PLC ve Kontrol Sistemleri");
        if (plc != null)
        {
            var siemensId = Guid.NewGuid();
            var omronId = Guid.NewGuid();
            var mitsubishiId = Guid.NewGuid();
            deepCategories.Add(new Category { Id = siemensId, Name = "Siemens", ParentId = plc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = omronId, Name = "Omron", ParentId = plc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = mitsubishiId, Name = "Mitsubishi", ParentId = plc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Allen Bradley", ParentId = plc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Schneider", ParentId = plc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "ABB", ParentId = plc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Delta", ParentId = plc.Id, IsActive = true, CreatedDate = DateTime.UtcNow });

            // Siemens > Modeller
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "S7-200", ParentId = siemensId, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "S7-300", ParentId = siemensId, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "S7-1200", ParentId = siemensId, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "S7-1500", ParentId = siemensId, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Logo!", ParentId = siemensId, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== SENSÖR MARKALARI =====
        var sensorler = level2Categories.FirstOrDefault(c => c.Name == "Sensörler");
        if (sensorler != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Fotosel Sensörler", ParentId = sensorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Endüktif Sensörler", ParentId = sensorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Kapasitif Sensörler", ParentId = sensorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Basınç Sensörleri", ParentId = sensorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Sıcaklık Sensörleri", ParentId = sensorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Ultrasonik Sensörler", ParentId = sensorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Encoder", ParentId = sensorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== MOTOR SÜRÜCÜ MARKALARI =====
        var motorSurucu = level2Categories.FirstOrDefault(c => c.Name == "Motor Sürücüler (Invertör)");
        if (motorSurucu != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Siemens", ParentId = motorSurucu.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "ABB", ParentId = motorSurucu.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Schneider", ParentId = motorSurucu.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Delta", ParentId = motorSurucu.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Danfoss", ParentId = motorSurucu.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Yaskawa", ParentId = motorSurucu.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Mitsubishi", ParentId = motorSurucu.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== HİDROLİK POMPA TİPLERİ =====
        var hidrolikPompa = level2Categories.FirstOrDefault(c => c.Name == "Hidrolik Pompalar");
        if (hidrolikPompa != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Dişli Pompalar", ParentId = hidrolikPompa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Paletli Pompalar", ParentId = hidrolikPompa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Pistonlu Pompalar", ParentId = hidrolikPompa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "El Pompaları", ParentId = hidrolikPompa.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== RULMAN MARKALARI =====
        var bilyaliRulman = level2Categories.FirstOrDefault(c => c.Name == "Bilyalı Rulmanlar");
        if (bilyaliRulman != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "SKF", ParentId = bilyaliRulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "FAG", ParentId = bilyaliRulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "NSK", ParentId = bilyaliRulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "NTN", ParentId = bilyaliRulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "INA", ParentId = bilyaliRulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Koyo", ParentId = bilyaliRulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Timken", ParentId = bilyaliRulman.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== KAYIŞ MARKALARI =====
        var kayislar = level2Categories.FirstOrDefault(c => c.Name == "Kayışlar");
        if (kayislar != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "V Kayışlar", ParentId = kayislar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Düz Kayışlar", ParentId = kayislar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Trapezoidal Kayışlar", ParentId = kayislar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Zamanlama Kayışları", ParentId = kayislar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Gates", ParentId = kayislar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Optibelt", ParentId = kayislar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Continental", ParentId = kayislar.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
        }

        // ===== REDÜKTÖR MARKALARI =====
        var reduktorler = level2Categories.FirstOrDefault(c => c.Name == "Redüktörler");
        if (reduktorler != null)
        {
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "SEW", ParentId = reduktorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Nord", ParentId = reduktorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Bonfiglioli", ParentId = reduktorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Rossi", ParentId = reduktorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Siti", ParentId = reduktorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
            deepCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Yılmaz Redüktör", ParentId = reduktorler.Id, IsActive = true, CreatedDate = DateTime.UtcNow });
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

        // PLC/Otomasyon ürünleri -> Elektrik ve Enerji
        if (titleLower.Contains("plc") || titleLower.Contains("siemens") || titleLower.Contains("omron") ||
            titleLower.Contains("fotosel") || titleLower.Contains("sensör") || titleLower.Contains("invertör") ||
            titleLower.Contains("pano"))
        {
            return categories.FirstOrDefault(c => c.Name == "PLC ve Kontrol Sistemleri")
                ?? categories.FirstOrDefault(c => c.Name == "Elektrik ve Enerji");
        }

        // Hidrolik ürünleri -> Hidrolik Pnömatik
        if (titleLower.Contains("hidrolik") || titleLower.Contains("silindir") ||
            titleLower.Contains("valf") || titleLower.Contains("tank"))
        {
            return categories.FirstOrDefault(c => c.Name == "Hidrolik Pompalar")
                ?? categories.FirstOrDefault(c => c.Name == "Hidrolik Pnömatik");
        }

        // Pompalar -> Makina > Pompa Modelleri
        if (titleLower.Contains("pompa"))
        {
            return categories.FirstOrDefault(c => c.Name == "Pompa Modelleri")
                ?? categories.FirstOrDefault(c => c.Name == "Makina");
        }

        // CNC/Talaşlı imalat
        if (titleLower.Contains("cnc") || titleLower.Contains("torna") || titleLower.Contains("freze") ||
            titleLower.Contains("taşlama"))
        {
            return categories.FirstOrDefault(c => c.Name == "CNC Makinaları")
                ?? categories.FirstOrDefault(c => c.Name == "Makina");
        }

        // Rulman
        if (titleLower.Contains("rulman") || titleLower.Contains("skf") || titleLower.Contains("fag") ||
            titleLower.Contains("ina") || titleLower.Contains("nsk"))
        {
            return categories.FirstOrDefault(c => c.Name == "Bilyalı Rulmanlar")
                ?? categories.FirstOrDefault(c => c.Name == "Rulman ve Transmisyon");
        }

        // Metal işleme / Kaynak
        if (titleLower.Contains("kaynak") || titleLower.Contains("argon") || titleLower.Contains("mig") ||
            titleLower.Contains("tig"))
        {
            return categories.FirstOrDefault(c => c.Name == "Kaynak Makineleri")
                ?? categories.FirstOrDefault(c => c.Name == "Makina");
        }

        // Lazer kesim
        if (titleLower.Contains("lazer") || titleLower.Contains("kesim"))
        {
            return categories.FirstOrDefault(c => c.Name == "Lazer Kesim Makinası")
                ?? categories.FirstOrDefault(c => c.Name == "Makina");
        }

        // Pres
        if (titleLower.Contains("pres") || titleLower.Contains("abkant") || titleLower.Contains("eksantrik"))
        {
            return categories.FirstOrDefault(c => c.Name == "Pres Makineleri")
                ?? categories.FirstOrDefault(c => c.Name == "Makina");
        }

        // Kompresör
        if (titleLower.Contains("kompresör"))
        {
            return categories.FirstOrDefault(c => c.Name == "Kompresör")
                ?? categories.FirstOrDefault(c => c.Name == "Makina");
        }

        // Redüktör
        if (titleLower.Contains("redüktör"))
        {
            return categories.FirstOrDefault(c => c.Name == "Redüktörler")
                ?? categories.FirstOrDefault(c => c.Name == "Rulman ve Transmisyon");
        }

        // Konveyör / İş makineleri
        if (titleLower.Contains("konveyör") || titleLower.Contains("forklift") || titleLower.Contains("transpalet"))
        {
            return categories.FirstOrDefault(c => c.Name == "Konveyör Sistemleri")
                ?? categories.FirstOrDefault(c => c.Name == "İş Makineleri");
        }

        // Hortum
        if (titleLower.Contains("hortum"))
        {
            return categories.FirstOrDefault(c => c.Name == "Hidrolik Hortum")
                ?? categories.FirstOrDefault(c => c.Name == "Hortum ve Bağlantı");
        }

        // Yedek parça / Genel
        if (titleLower.Contains("yedek") || titleLower.Contains("parça") || titleLower.Contains("filtre") ||
            titleLower.Contains("conta"))
        {
            return categories.FirstOrDefault(c => c.Name == "Yedek Parça");
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
            "Hidrolik" => categories.FirstOrDefault(c => c.Name == "Hidrolik Pompalar") ?? categories.FirstOrDefault(c => c.Name == "Hidrolik Pnömatik"),
            "CNC" => categories.FirstOrDefault(c => c.Name == "CNC Makinaları") ?? categories.FirstOrDefault(c => c.Name == "Makina"),
            "Rulman" => categories.FirstOrDefault(c => c.Name == "Bilyalı Rulmanlar") ?? categories.FirstOrDefault(c => c.Name == "Rulman ve Transmisyon"),
            "PLC" => categories.FirstOrDefault(c => c.Name == "PLC ve Kontrol Sistemleri") ?? categories.FirstOrDefault(c => c.Name == "Elektrik ve Enerji"),
            "Kaynak" => categories.FirstOrDefault(c => c.Name == "Kaynak Makineleri") ?? categories.FirstOrDefault(c => c.Name == "Makina"),
            "Yedek" => categories.FirstOrDefault(c => c.Name == "Motor Yedek Parçaları") ?? categories.FirstOrDefault(c => c.Name == "Yedek Parça"),
            "Pompa" => categories.FirstOrDefault(c => c.Name == "Pompa Modelleri") ?? categories.FirstOrDefault(c => c.Name == "Makina"),
            "Pres" => categories.FirstOrDefault(c => c.Name == "Pres Makineleri") ?? categories.FirstOrDefault(c => c.Name == "Makina"),
            "Lazer" => categories.FirstOrDefault(c => c.Name == "Lazer Kesim Makinası") ?? categories.FirstOrDefault(c => c.Name == "Makina"),
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
            // JSON dosyasını oku - birden fazla olası path dene
            var possiblePaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "locations.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "data", "locations.json"),
                "/app/data/locations.json",  // Docker container path
                Path.Combine(AppContext.BaseDirectory, "data", "locations.json")
            };
            
            string? jsonPath = null;
            foreach (var path in possiblePaths)
            {
                Console.WriteLine($"Checking for location data at: {path}");
                if (File.Exists(path))
                {
                    jsonPath = path;
                    Console.WriteLine($"Found location data at: {path}");
                    break;
                }
            }
            
            if (jsonPath == null)
            {
                Console.WriteLine("Location data file not found at any expected location.");
                Console.WriteLine("Tried paths:");
                foreach (var path in possiblePaths)
                {
                    Console.WriteLine($"  - {path}");
                }
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
