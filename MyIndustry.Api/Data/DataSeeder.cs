using MyIndustry.Domain.Aggregate;
using MyIndustry.Domain.ValueObjects;
using MyIndustry.Repository.DbContext;

namespace MyIndustry.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(MyIndustryDbContext context)
    {
        // Eğer zaten veri varsa sadece derin kategorileri kontrol et ve mevcut servisleri düzelt
        if (context.Categories.Any())
        {
            Console.WriteLine("Database already has data. Checking for deep categories...");
            await SeedDeepCategoriesIfMissing(context);
            await FixServiceCategories(context);
            return;
        }

        Console.WriteLine("Seeding database with dummy data...");

        // 1. Kategoriler
        var categories = CreateCategories();
        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {categories.Count} categories.");

        // Alt kategorileri oluştur (2. seviye)
        var subCategories = CreateSubCategories(categories);
        await context.Categories.AddRangeAsync(subCategories);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {subCategories.Count} sub-categories (level 2).");

        // Daha derin alt kategorileri oluştur (3., 4., 5. seviye)
        var deeperCategories = CreateDeeperCategories(subCategories);
        await context.Categories.AddRangeAsync(deeperCategories);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {deeperCategories.Count} deeper categories (level 3-5).");

        // 2. Subscription Plans
        var plans = CreateSubscriptionPlans();
        await context.SubscriptionPlans.AddRangeAsync(plans);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {plans.Count} subscription plans.");

        // 3. Sellers
        var sellers = CreateSellers();
        await context.Sellers.AddRangeAsync(sellers);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {sellers.Count} sellers.");

        // 4. Seller Infos
        var sellerInfos = CreateSellerInfos(sellers);
        await context.SellerInfos.AddRangeAsync(sellerInfos);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {sellerInfos.Count} seller infos.");

        // 5. Services
        var allCategories = categories.Concat(subCategories).Concat(deeperCategories).ToList();
        var services = CreateServices(sellers, allCategories);
        await context.Services.AddRangeAsync(services);
        await context.SaveChangesAsync();
        Console.WriteLine($"Created {services.Count} services.");

        Console.WriteLine("Database seeding completed!");
    }

    private static async Task SeedDeepCategoriesIfMissing(MyIndustryDbContext context)
    {
        var addedCount = 0;
        
        // Motor Yedek Parçaları altına derin kategoriler
        var motorYedek = context.Categories.FirstOrDefault(c => c.Name == "Motor Yedek Parçaları" && c.IsActive);
        if (motorYedek != null)
        {
            // Çekici var mı?
            if (!context.Categories.Any(c => c.Name == "Çekici" && c.ParentId == motorYedek.Id))
            {
                var cekiciId = Guid.NewGuid();
                var kamyonId = Guid.NewGuid();
                var otobusId = Guid.NewGuid();
                
                // 3. seviye - Araç Tipleri
                context.Categories.AddRange(new[]
                {
                    new Category { Id = cekiciId, Name = "Çekici", Description = "Çekici motor yedek parçaları", IsActive = true, ParentId = motorYedek.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = kamyonId, Name = "Kamyon", Description = "Kamyon motor yedek parçaları", IsActive = true, ParentId = motorYedek.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = otobusId, Name = "Otobüs", Description = "Otobüs motor yedek parçaları", IsActive = true, ParentId = motorYedek.Id, CreatedDate = DateTime.UtcNow }
                });
                
                // 4. seviye - Çekici Markaları
                var scaniaId = Guid.NewGuid();
                var volvoId = Guid.NewGuid();
                var mercedesId = Guid.NewGuid();
                
                context.Categories.AddRange(new[]
                {
                    new Category { Id = scaniaId, Name = "Scania", Description = "Scania çekici yedek parçaları", IsActive = true, ParentId = cekiciId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = volvoId, Name = "Volvo", Description = "Volvo çekici yedek parçaları", IsActive = true, ParentId = cekiciId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = mercedesId, Name = "Mercedes", Description = "Mercedes çekici yedek parçaları", IsActive = true, ParentId = cekiciId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "MAN", Description = "MAN çekici yedek parçaları", IsActive = true, ParentId = cekiciId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "DAF", Description = "DAF çekici yedek parçaları", IsActive = true, ParentId = cekiciId, CreatedDate = DateTime.UtcNow }
                });
                
                // 5. seviye - Scania Modelleri
                context.Categories.AddRange(new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "R410", Description = "Scania R410 yedek parçaları", IsActive = true, ParentId = scaniaId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "R450", Description = "Scania R450 yedek parçaları", IsActive = true, ParentId = scaniaId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "R500", Description = "Scania R500 yedek parçaları", IsActive = true, ParentId = scaniaId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "S500", Description = "Scania S500 yedek parçaları", IsActive = true, ParentId = scaniaId, CreatedDate = DateTime.UtcNow }
                });
                
                // 5. seviye - Volvo Modelleri
                context.Categories.AddRange(new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "FH16", Description = "Volvo FH16 yedek parçaları", IsActive = true, ParentId = volvoId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "FH500", Description = "Volvo FH500 yedek parçaları", IsActive = true, ParentId = volvoId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "FM", Description = "Volvo FM yedek parçaları", IsActive = true, ParentId = volvoId, CreatedDate = DateTime.UtcNow }
                });
                
                // 5. seviye - Mercedes Modelleri
                context.Categories.AddRange(new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "Actros", Description = "Mercedes Actros yedek parçaları", IsActive = true, ParentId = mercedesId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Arocs", Description = "Mercedes Arocs yedek parçaları", IsActive = true, ParentId = mercedesId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Axor", Description = "Mercedes Axor yedek parçaları", IsActive = true, ParentId = mercedesId, CreatedDate = DateTime.UtcNow }
                });
                
                addedCount += 18;
                Console.WriteLine("Added deep categories for Motor Yedek Parçaları");
            }
        }
        
        // Hidrolik Pompalar altına derin kategoriler
        var hidrolikPompa = context.Categories.FirstOrDefault(c => c.Name == "Hidrolik Pompalar" && c.IsActive);
        if (hidrolikPompa != null)
        {
            if (!context.Categories.Any(c => c.Name == "Dişli Pompalar" && c.ParentId == hidrolikPompa.Id))
            {
                var disliPompaId = Guid.NewGuid();
                
                // 3. seviye - Pompa Tipleri
                context.Categories.AddRange(new[]
                {
                    new Category { Id = disliPompaId, Name = "Dişli Pompalar", Description = "Dişli tip hidrolik pompalar", IsActive = true, ParentId = hidrolikPompa.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Pistonlu Pompalar", Description = "Pistonlu tip hidrolik pompalar", IsActive = true, ParentId = hidrolikPompa.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Paletli Pompalar", Description = "Paletli tip hidrolik pompalar", IsActive = true, ParentId = hidrolikPompa.Id, CreatedDate = DateTime.UtcNow }
                });
                
                // 4. seviye - Dişli Pompa Markaları
                context.Categories.AddRange(new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "Bosch Rexroth", Description = "Bosch Rexroth dişli pompalar", IsActive = true, ParentId = disliPompaId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Parker", Description = "Parker dişli pompalar", IsActive = true, ParentId = disliPompaId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Casappa", Description = "Casappa dişli pompalar", IsActive = true, ParentId = disliPompaId, CreatedDate = DateTime.UtcNow }
                });
                
                addedCount += 6;
                Console.WriteLine("Added deep categories for Hidrolik Pompalar");
            }
        }
        
        // PLC ve Kontrol altına derin kategoriler
        var plc = context.Categories.FirstOrDefault(c => c.Name == "PLC ve Kontrol" && c.IsActive);
        if (plc != null)
        {
            if (!context.Categories.Any(c => c.Name == "Siemens" && c.ParentId == plc.Id))
            {
                var siemensId = Guid.NewGuid();
                
                // 3. seviye - PLC Markaları
                context.Categories.AddRange(new[]
                {
                    new Category { Id = siemensId, Name = "Siemens", Description = "Siemens PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Omron", Description = "Omron PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Allen Bradley", Description = "Allen Bradley PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Mitsubishi", Description = "Mitsubishi PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow }
                });
                
                // 4. seviye - Siemens Serileri
                context.Categories.AddRange(new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "S7-1200", Description = "Siemens S7-1200 serisi", IsActive = true, ParentId = siemensId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "S7-1500", Description = "Siemens S7-1500 serisi", IsActive = true, ParentId = siemensId, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "S7-300", Description = "Siemens S7-300 serisi", IsActive = true, ParentId = siemensId, CreatedDate = DateTime.UtcNow }
                });
                
                addedCount += 7;
                Console.WriteLine("Added deep categories for PLC ve Kontrol");
            }
        }
        
        if (addedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Seeded {addedCount} deep categories.");
        }
        else
        {
            Console.WriteLine("Deep categories already exist. Nothing to seed.");
        }
    }

    private static async Task FixServiceCategories(MyIndustryDbContext context)
    {
        var categories = context.Categories.ToList();
        var services = context.Services.ToList();
        var fixedCount = 0;

        foreach (var service in services)
        {
            var titleLower = service.Title?.ToLower() ?? "";
            Category? targetCategory = null;

            // PLC/Otomasyon ürünleri
            if (titleLower.Contains("plc") || titleLower.Contains("siemens") || titleLower.Contains("omron") || 
                titleLower.Contains("fotosel") || titleLower.Contains("sensör") || titleLower.Contains("invertör") ||
                titleLower.Contains("pano"))
            {
                targetCategory = categories.FirstOrDefault(c => c.Name == "PLC ve Kontrol") 
                              ?? categories.FirstOrDefault(c => c.Name == "Elektrik ve Otomasyon");
            }
            // Hidrolik ürünleri
            else if (titleLower.Contains("hidrolik") || titleLower.Contains("pompa") || titleLower.Contains("silindir") || 
                     titleLower.Contains("valf") || titleLower.Contains("tank") || titleLower.Contains("hortum"))
            {
                targetCategory = categories.FirstOrDefault(c => c.Name == "Hidrolik Pompalar") 
                              ?? categories.FirstOrDefault(c => c.Name == "Hidrolik Sistemler");
            }
            // CNC/Talaşlı imalat
            else if (titleLower.Contains("cnc") || titleLower.Contains("torna") || titleLower.Contains("freze") || 
                     titleLower.Contains("taşlama"))
            {
                targetCategory = categories.FirstOrDefault(c => c.Name == "CNC ve Talaşlı İmalat");
            }
            // Rulman
            else if (titleLower.Contains("rulman") || titleLower.Contains("skf") || titleLower.Contains("fag") || 
                     titleLower.Contains("ina"))
            {
                targetCategory = categories.FirstOrDefault(c => c.Name == "Rulman ve Transmisyon");
            }
            // Metal işleme / Kaynak
            else if (titleLower.Contains("kaynak") || titleLower.Contains("lazer") || titleLower.Contains("kesim") || 
                     titleLower.Contains("bükme") || titleLower.Contains("konstrüksiyon"))
            {
                targetCategory = categories.FirstOrDefault(c => c.Name == "Kaynak ve Metal İşleme");
            }
            // Yedek parça / Genel
            else if (titleLower.Contains("redüktör") || titleLower.Contains("kompresör") || titleLower.Contains("konveyör") || 
                     titleLower.Contains("zincir"))
            {
                targetCategory = categories.FirstOrDefault(c => c.Name == "Motor Yedek Parçaları") 
                              ?? categories.FirstOrDefault(c => c.Name == "Yedek Parça");
            }

            if (targetCategory != null && service.CategoryId != targetCategory.Id)
            {
                Console.WriteLine($"Fixing: '{service.Title}' -> {targetCategory.Name}");
                service.CategoryId = targetCategory.Id;
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            await context.SaveChangesAsync();
            Console.WriteLine($"Fixed {fixedCount} service categories.");
        }
        else
        {
            Console.WriteLine("All service categories are correct.");
        }
    }

    private static List<Category> CreateCategories()
    {
        return new List<Category>
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Yedek Parça",
                Description = "Endüstriyel yedek parçalar ve komponentler",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Makine ve Ekipman",
                Description = "Endüstriyel makineler ve ekipmanlar",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Hidrolik Sistemler",
                Description = "Hidrolik pompalar, silindirler ve sistemler",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Pnömatik Sistemler",
                Description = "Pnömatik ekipmanlar ve komponentler",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Elektrik ve Otomasyon",
                Description = "Elektrik malzemeleri ve otomasyon sistemleri",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "CNC ve Talaşlı İmalat",
                Description = "CNC işleme ve talaşlı imalat hizmetleri",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Kaynak ve Metal İşleme",
                Description = "Kaynak ve metal işleme hizmetleri",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Rulman ve Transmisyon",
                Description = "Rulmanlar, kayışlar ve transmisyon elemanları",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };
    }

    private static List<Category> CreateSubCategories(List<Category> parentCategories)
    {
        var subCategories = new List<Category>();
        
        // Yedek Parça alt kategorileri
        var yedekParca = parentCategories.First(c => c.Name == "Yedek Parça");
        subCategories.AddRange(new[]
        {
            new Category { Id = Guid.NewGuid(), Name = "Motor Yedek Parçaları", Description = "Elektrik ve dizel motor parçaları", IsActive = true, ParentId = yedekParca.Id, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Pompa Yedek Parçaları", Description = "Her türlü pompa yedek parçası", IsActive = true, ParentId = yedekParca.Id, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Kompresör Parçaları", Description = "Kompresör yedek parçaları", IsActive = true, ParentId = yedekParca.Id, CreatedDate = DateTime.UtcNow },
        });

        // Hidrolik alt kategorileri
        var hidrolik = parentCategories.First(c => c.Name == "Hidrolik Sistemler");
        subCategories.AddRange(new[]
        {
            new Category { Id = Guid.NewGuid(), Name = "Hidrolik Pompalar", Description = "Dişli, pistonlu ve paletli pompalar", IsActive = true, ParentId = hidrolik.Id, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Hidrolik Silindirler", Description = "Tek ve çift etkili silindirler", IsActive = true, ParentId = hidrolik.Id, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Hidrolik Valfler", Description = "Yön kontrol ve basınç valfleri", IsActive = true, ParentId = hidrolik.Id, CreatedDate = DateTime.UtcNow },
        });

        // CNC alt kategorileri
        var cnc = parentCategories.First(c => c.Name == "CNC ve Talaşlı İmalat");
        subCategories.AddRange(new[]
        {
            new Category { Id = Guid.NewGuid(), Name = "CNC Torna", Description = "CNC torna işleme hizmetleri", IsActive = true, ParentId = cnc.Id, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "CNC Freze", Description = "CNC freze işleme hizmetleri", IsActive = true, ParentId = cnc.Id, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Taşlama", Description = "Hassas taşlama işlemleri", IsActive = true, ParentId = cnc.Id, CreatedDate = DateTime.UtcNow },
        });

        // Elektrik alt kategorileri
        var elektrik = parentCategories.First(c => c.Name == "Elektrik ve Otomasyon");
        subCategories.AddRange(new[]
        {
            new Category { Id = Guid.NewGuid(), Name = "PLC ve Kontrol", Description = "PLC sistemleri ve kontrol panelleri", IsActive = true, ParentId = elektrik.Id, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Sensörler", Description = "Endüstriyel sensörler", IsActive = true, ParentId = elektrik.Id, CreatedDate = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Motor Sürücüler", Description = "Frekans invertörleri ve sürücüler", IsActive = true, ParentId = elektrik.Id, CreatedDate = DateTime.UtcNow },
        });

        return subCategories;
    }

    private static List<Category> CreateDeeperCategories(List<Category> level2Categories)
    {
        var deeperCategories = new List<Category>();
        
        // Motor Yedek Parçaları > Çekici Markaları (3. seviye)
        var motorYedek = level2Categories.FirstOrDefault(c => c.Name == "Motor Yedek Parçaları");
        if (motorYedek != null)
        {
            var cekiciId = Guid.NewGuid();
            var kamyonId = Guid.NewGuid();
            var otoId = Guid.NewGuid();
            
            // 3. seviye - Araç Tipleri
            deeperCategories.Add(new Category { Id = cekiciId, Name = "Çekici", Description = "Çekici motor yedek parçaları", IsActive = true, ParentId = motorYedek.Id, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = kamyonId, Name = "Kamyon", Description = "Kamyon motor yedek parçaları", IsActive = true, ParentId = motorYedek.Id, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = otoId, Name = "Otobüs", Description = "Otobüs motor yedek parçaları", IsActive = true, ParentId = motorYedek.Id, CreatedDate = DateTime.UtcNow });
            
            // 4. seviye - Çekici Markaları
            var scaniaId = Guid.NewGuid();
            var volvoId = Guid.NewGuid();
            var mercedesId = Guid.NewGuid();
            var manId = Guid.NewGuid();
            var dafId = Guid.NewGuid();
            
            deeperCategories.Add(new Category { Id = scaniaId, Name = "Scania", Description = "Scania çekici yedek parçaları", IsActive = true, ParentId = cekiciId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = volvoId, Name = "Volvo", Description = "Volvo çekici yedek parçaları", IsActive = true, ParentId = cekiciId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = mercedesId, Name = "Mercedes", Description = "Mercedes çekici yedek parçaları", IsActive = true, ParentId = cekiciId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = manId, Name = "MAN", Description = "MAN çekici yedek parçaları", IsActive = true, ParentId = cekiciId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = dafId, Name = "DAF", Description = "DAF çekici yedek parçaları", IsActive = true, ParentId = cekiciId, CreatedDate = DateTime.UtcNow });
            
            // 5. seviye - Scania Modelleri
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "R410", Description = "Scania R410 yedek parçaları", IsActive = true, ParentId = scaniaId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "R450", Description = "Scania R450 yedek parçaları", IsActive = true, ParentId = scaniaId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "R500", Description = "Scania R500 yedek parçaları", IsActive = true, ParentId = scaniaId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "S500", Description = "Scania S500 yedek parçaları", IsActive = true, ParentId = scaniaId, CreatedDate = DateTime.UtcNow });
            
            // 5. seviye - Volvo Modelleri
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "FH16", Description = "Volvo FH16 yedek parçaları", IsActive = true, ParentId = volvoId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "FH500", Description = "Volvo FH500 yedek parçaları", IsActive = true, ParentId = volvoId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "FM", Description = "Volvo FM yedek parçaları", IsActive = true, ParentId = volvoId, CreatedDate = DateTime.UtcNow });
            
            // 5. seviye - Mercedes Modelleri
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Actros", Description = "Mercedes Actros yedek parçaları", IsActive = true, ParentId = mercedesId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Arocs", Description = "Mercedes Arocs yedek parçaları", IsActive = true, ParentId = mercedesId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Axor", Description = "Mercedes Axor yedek parçaları", IsActive = true, ParentId = mercedesId, CreatedDate = DateTime.UtcNow });
        }
        
        // Hidrolik Pompalar > Pompa Tipleri (3. seviye)
        var hidrolikPompa = level2Categories.FirstOrDefault(c => c.Name == "Hidrolik Pompalar");
        if (hidrolikPompa != null)
        {
            var disliPompaId = Guid.NewGuid();
            var pistonluPompaId = Guid.NewGuid();
            
            deeperCategories.Add(new Category { Id = disliPompaId, Name = "Dişli Pompalar", Description = "Dişli tip hidrolik pompalar", IsActive = true, ParentId = hidrolikPompa.Id, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = pistonluPompaId, Name = "Pistonlu Pompalar", Description = "Pistonlu tip hidrolik pompalar", IsActive = true, ParentId = hidrolikPompa.Id, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Paletli Pompalar", Description = "Paletli tip hidrolik pompalar", IsActive = true, ParentId = hidrolikPompa.Id, CreatedDate = DateTime.UtcNow });
            
            // 4. seviye - Marka bazlı
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Bosch Rexroth", Description = "Bosch Rexroth dişli pompalar", IsActive = true, ParentId = disliPompaId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Parker", Description = "Parker dişli pompalar", IsActive = true, ParentId = disliPompaId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Casappa", Description = "Casappa dişli pompalar", IsActive = true, ParentId = disliPompaId, CreatedDate = DateTime.UtcNow });
        }
        
        // PLC ve Kontrol > Marka bazlı (3. seviye)
        var plc = level2Categories.FirstOrDefault(c => c.Name == "PLC ve Kontrol");
        if (plc != null)
        {
            var siemensId = Guid.NewGuid();
            var omronId = Guid.NewGuid();
            
            deeperCategories.Add(new Category { Id = siemensId, Name = "Siemens", Description = "Siemens PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = omronId, Name = "Omron", Description = "Omron PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Allen Bradley", Description = "Allen Bradley PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "Mitsubishi", Description = "Mitsubishi PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow });
            
            // 4. seviye - Siemens serileri
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "S7-1200", Description = "Siemens S7-1200 serisi", IsActive = true, ParentId = siemensId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "S7-1500", Description = "Siemens S7-1500 serisi", IsActive = true, ParentId = siemensId, CreatedDate = DateTime.UtcNow });
            deeperCategories.Add(new Category { Id = Guid.NewGuid(), Name = "S7-300", Description = "Siemens S7-300 serisi", IsActive = true, ParentId = siemensId, CreatedDate = DateTime.UtcNow });
        }
        
        return deeperCategories;
    }

    private static List<SubscriptionPlan> CreateSubscriptionPlans()
    {
        return new List<SubscriptionPlan>
        {
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Ücretsiz",
                Description = "Başlangıç paketi - Sınırlı ilan hakkı",
                SubscriptionType = SubscriptionType.Free,
                MonthlyPrice = 0,
                MonthlyPostLimit = 3,
                PostDurationInDays = 30,
                FeaturedPostLimit = 0,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Standart",
                Description = "Küçük işletmeler için ideal paket",
                SubscriptionType = SubscriptionType.Standard,
                MonthlyPrice = 29900, // 299 TL (kuruş cinsinden)
                MonthlyPostLimit = 15,
                PostDurationInDays = 45,
                FeaturedPostLimit = 2,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Premium",
                Description = "Orta ölçekli işletmeler için gelişmiş paket",
                SubscriptionType = SubscriptionType.Premium,
                MonthlyPrice = 59900, // 599 TL
                MonthlyPostLimit = 50,
                PostDurationInDays = 60,
                FeaturedPostLimit = 10,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Kurumsal",
                Description = "Büyük firmalar için sınırsız paket",
                SubscriptionType = SubscriptionType.Corporate,
                MonthlyPrice = 149900, // 1499 TL
                MonthlyPostLimit = 999,
                PostDurationInDays = 90,
                FeaturedPostLimit = 50,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };
    }

    private static List<Seller> CreateSellers()
    {
        return new List<Seller>
        {
            new Seller
            {
                Id = Guid.NewGuid(),
                IdentityNumber = "1234567890",
                Title = "Anadolu Hidrolik",
                Description = "25 yıllık tecrübemizle hidrolik sistemler ve yedek parça tedariğinde lider firmayız.",
                Sector = SellerSector.IronMongery,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Seller
            {
                Id = Guid.NewGuid(),
                IdentityNumber = "2345678901",
                Title = "Marmara CNC",
                Description = "Modern CNC tezgahlarımızla hassas işçilik ve hızlı teslimat garantisi.",
                Sector = SellerSector.IronMongery,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Seller
            {
                Id = Guid.NewGuid(),
                IdentityNumber = "3456789012",
                Title = "Ege Rulman",
                Description = "Türkiye'nin en geniş rulman stoku ile hizmetinizdeyiz.",
                Sector = SellerSector.IronMongery,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Seller
            {
                Id = Guid.NewGuid(),
                IdentityNumber = "4567890123",
                Title = "Akdeniz Otomasyon",
                Description = "Endüstriyel otomasyon sistemleri kurulum ve bakım hizmetleri.",
                Sector = SellerSector.IronMongery,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Seller
            {
                Id = Guid.NewGuid(),
                IdentityNumber = "5678901234",
                Title = "Karadeniz Metal",
                Description = "Metal işleme, kaynak ve imalat hizmetlerinde 30 yıllık deneyim.",
                Sector = SellerSector.Dumper,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Seller
            {
                Id = Guid.NewGuid(),
                IdentityNumber = "6789012345",
                Title = "İç Anadolu Makine",
                Description = "Endüstriyel makine satış, servis ve yedek parça hizmetleri.",
                Sector = SellerSector.IronMongery,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        };
    }

    private static List<SellerInfo> CreateSellerInfos(List<Seller> sellers)
    {
        var infos = new List<SellerInfo>();
        var cities = new[] { "İstanbul", "Ankara", "İzmir", "Bursa", "Antalya", "Konya" };
        var domains = new[] { "anadoluhidrolik", "marmaracnc", "egerulman", "akdenizotomasyon", "karadenizemetal", "icanadolumakine" };

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
        var random = new Random(42); // Sabit seed ile tekrarlanabilir sonuçlar

        var serviceTemplates = new[]
        {
            // Hidrolik
            ("Hidrolik Pompa - Dişli Tip 50 Lt/dk", "Yüksek basınç dişli pompa, 250 bar çalışma basıncı, döküm gövde", 450000, 7),
            ("Hidrolik Silindir Ø80x500 mm", "Çift etkili hidrolik silindir, krom kaplı mil, sertleştirilmiş burçlar", 320000, 10),
            ("Hidrolik Yön Kontrol Valfi", "4/3 yön kontrol valfi, 24V DC bobin, P-max 320 bar", 180000, 5),
            ("Hidrolik Tank 100 Lt", "Çelik tank, seviye göstergesi ve hava filtresi dahil", 250000, 14),
            ("Hidrolik Hortum Seti", "DN12 yüksek basınç hortum, 2 metre, rekor dahil", 45000, 3),
            
            // CNC
            ("CNC Torna İşleme Hizmeti", "Hassas CNC torna işleme, ±0.01 tolerans, her türlü malzeme", 15000, 5),
            ("CNC Freze İşleme", "3 ve 5 eksen CNC freze, karmaşık geometriler, prototip üretim", 25000, 7),
            ("Hassas Taşlama Hizmeti", "Silindirik ve düzlem taşlama, Ra 0.4 yüzey kalitesi", 20000, 4),
            ("Özel Parça İmalatı", "Teknik resme göre özel parça üretimi, küçük ve büyük seri", 50000, 14),
            
            // Rulman
            ("SKF 6205-2RS Rulman", "Orijinal SKF, çift taraflı kauçuk conta, yüksek hız", 8500, 2),
            ("FAG 32210 Konik Rulman", "Konik makaralı rulman, ağır yük kapasitesi", 24000, 3),
            ("INA HK2516 İğne Rulman", "İğne makaralı rulman, kompakt tasarım", 6500, 2),
            ("Rulman Montaj Seti", "Isıtıcı, çektirme ve montaj aletleri seti", 350000, 7),
            
            // Elektrik/Otomasyon
            ("Siemens S7-1200 PLC", "CPU 1214C DC/DC/DC, 14 DI / 10 DO, Ethernet", 850000, 5),
            ("Omron Fotosel Sensör", "E3Z serisi, 4m algılama mesafesi, PNP çıkış", 32000, 3),
            ("Delta VFD Frekans İnvertör 7.5kW", "380V 3 faz, vektör kontrol, RS485 haberleşme", 420000, 7),
            ("Endüstriyel Pano İmalatı", "Özel tasarım elektrik panosu, montaj ve kablolama dahil", 1500000, 21),
            
            // Metal İşleme
            ("Çelik Konstrüksiyon İmalatı", "Teknik projeye göre çelik konstrüksiyon, boyalı teslim", 5000000, 30),
            ("Lazer Kesim Hizmeti", "CNC lazer kesim, 20mm'e kadar sac, hassas kesim", 10000, 3),
            ("Argon Kaynak Hizmeti", "TIG/MIG kaynak, paslanmaz ve alüminyum", 25000, 5),
            ("Sac Bükme İşlemi", "CNC abkant pres, 4m boy kapasitesi", 8000, 2),
            
            // Genel Yedek Parça
            ("Redüktör Yedek Parça Seti", "SEW R47 serisi için komple tamir seti", 180000, 10),
            ("Kompresör Tamir Kiti", "Atlas Copco GA serisi için bakım seti", 95000, 7),
            ("Konveyör Bant Tamburu", "Ø400x800 mm, kauçuk kaplı, rulmanlar dahil", 280000, 14),
            ("Endüstriyel Zincir", "DIN 8187, 1 inç pitch, 5 metre", 45000, 3),
        };

        var imageUrls = new[]
        {
            "[\"https://images.unsplash.com/photo-1581092160562-40aa08e78837?w=800\",\"https://images.unsplash.com/photo-1581092918056-0c4c3acd3789?w=800\"]",
            "[\"https://images.unsplash.com/photo-1565193566173-7a0ee3dbe261?w=800\",\"https://images.unsplash.com/photo-1504917595217-d4dc5ebe6122?w=800\"]",
            "[\"https://images.unsplash.com/photo-1581092160607-ee67df17a6e0?w=800\"]",
            "[\"https://images.unsplash.com/photo-1537462715879-360eeb61a0ad?w=800\",\"https://images.unsplash.com/photo-1581092162384-8987c1d64718?w=800\"]",
        };

        // Kategori eşleştirme kuralları - servis adına göre uygun kategori bul
        Category FindBestCategory(string title)
        {
            var titleLower = title.ToLower();
            
            // PLC/Otomasyon ürünleri
            if (titleLower.Contains("plc") || titleLower.Contains("siemens") || titleLower.Contains("omron") || 
                titleLower.Contains("fotosel") || titleLower.Contains("sensör") || titleLower.Contains("invertör") ||
                titleLower.Contains("pano"))
            {
                var plcCategory = categories.FirstOrDefault(c => c.Name == "PLC ve Kontrol");
                if (plcCategory != null) return plcCategory;
                var elektrikCategory = categories.FirstOrDefault(c => c.Name == "Elektrik ve Otomasyon");
                if (elektrikCategory != null) return elektrikCategory;
            }
            
            // Hidrolik ürünleri
            if (titleLower.Contains("hidrolik") || titleLower.Contains("pompa") || titleLower.Contains("silindir") || 
                titleLower.Contains("valf") || titleLower.Contains("tank") || titleLower.Contains("hortum"))
            {
                var hidrolikPompa = categories.FirstOrDefault(c => c.Name == "Hidrolik Pompalar");
                if (hidrolikPompa != null) return hidrolikPompa;
                var hidrolik = categories.FirstOrDefault(c => c.Name == "Hidrolik Sistemler");
                if (hidrolik != null) return hidrolik;
            }
            
            // CNC/Talaşlı imalat
            if (titleLower.Contains("cnc") || titleLower.Contains("torna") || titleLower.Contains("freze") || 
                titleLower.Contains("taşlama"))
            {
                var cnc = categories.FirstOrDefault(c => c.Name == "CNC ve Talaşlı İmalat");
                if (cnc != null) return cnc;
            }
            
            // Rulman
            if (titleLower.Contains("rulman") || titleLower.Contains("skf") || titleLower.Contains("fag") || 
                titleLower.Contains("ina"))
            {
                var rulman = categories.FirstOrDefault(c => c.Name == "Rulman ve Transmisyon");
                if (rulman != null) return rulman;
            }
            
            // Metal işleme / Kaynak
            if (titleLower.Contains("kaynak") || titleLower.Contains("lazer") || titleLower.Contains("kesim") || 
                titleLower.Contains("bükme") || titleLower.Contains("konstrüksiyon"))
            {
                var metal = categories.FirstOrDefault(c => c.Name == "Kaynak ve Metal İşleme");
                if (metal != null) return metal;
            }
            
            // Yedek parça / Genel
            if (titleLower.Contains("redüktör") || titleLower.Contains("kompresör") || titleLower.Contains("konveyör") || 
                titleLower.Contains("zincir"))
            {
                var yedek = categories.FirstOrDefault(c => c.Name == "Motor Yedek Parçaları");
                if (yedek != null) return yedek;
                var yedekParca = categories.FirstOrDefault(c => c.Name == "Yedek Parça");
                if (yedekParca != null) return yedekParca;
            }
            
            // Varsayılan: alt kategorilerden rastgele
            var subCategories = categories.Where(c => c.ParentId != null).ToList();
            return subCategories[random.Next(subCategories.Count)];
        }

        foreach (var (title, desc, price, days) in serviceTemplates)
        {
            var seller = sellers[random.Next(sellers.Count)];
            var category = FindBestCategory(title);
            
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
                ImageUrls = imageUrls[random.Next(imageUrls.Length)],
                CreatedDate = DateTime.UtcNow.AddDays(-random.Next(1, 60))
            });
        }

        return services;
    }
}
