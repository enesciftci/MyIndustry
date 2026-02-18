using MediatR;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Handler.Category.CreateCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.CreateSubCategoryCommand;
using MyIndustry.ApplicationService.Handler.Category.GetCategoriesQuery;
using MyIndustry.ApplicationService.Handler.Category.GetMainCategoriesQuery;
using MyIndustry.Domain.Aggregate;
using MyIndustry.Repository.DbContext;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class CategoryController(IMediator mediator, MyIndustryDbContext dbContext) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(command, cancellationToken));
    }

    [HttpGet("list")]
    public async Task<IActionResult> Get([FromQuery] Guid? parentId, CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(new GetCategoriesQuery ()
        {
            ParentId = parentId
        } , cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> Update(CancellationToken cancellationToken)
    {
        return CreateResponse(null);
    }

    [HttpPost("subcategory")]
    public async Task<IActionResult> CreateSubCategory(CreateSubCategoryCommand command, CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(command, cancellationToken));
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree(CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(new GetCategoriesQuery2(), cancellationToken));
    }
    
    [HttpGet("main")]
    public async Task<IActionResult> GetMainCategories(CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(new GetMainCategoriesQuery(), cancellationToken));
    }
    
    [HttpGet("{parentId:guid}")]
    public async Task<IActionResult> GetSubCategories(Guid parentId, CancellationToken cancellationToken)
    {
        return CreateResponse(await mediator.Send(new GetCategoriesQuery2(){ParentId =parentId }, cancellationToken));
    }

    /// <summary>
    /// Mevcut kategorilere derin alt kategoriler ekler (3-5 seviye)
    /// </summary>
    [HttpPost("seed-deep-categories")]
    public async Task<IActionResult> SeedDeepCategories(CancellationToken cancellationToken)
    {
        var addedCount = 0;
        
        // Motor Yedek Parçaları kategorisini bul
        var motorYedek = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Name == "Motor Yedek Parçaları" && c.IsActive, cancellationToken);
            
        if (motorYedek != null)
        {
            // Çekici zaten var mı kontrol et
            var existingCekici = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name == "Çekici" && c.ParentId == motorYedek.Id, cancellationToken);
                
            if (existingCekici == null)
            {
                // 3. seviye - Araç Tipleri
                var cekici = new Category { Id = Guid.NewGuid(), Name = "Çekici", Description = "Çekici motor yedek parçaları", IsActive = true, ParentId = motorYedek.Id, CreatedDate = DateTime.UtcNow };
                var kamyon = new Category { Id = Guid.NewGuid(), Name = "Kamyon", Description = "Kamyon motor yedek parçaları", IsActive = true, ParentId = motorYedek.Id, CreatedDate = DateTime.UtcNow };
                var otobus = new Category { Id = Guid.NewGuid(), Name = "Otobüs", Description = "Otobüs motor yedek parçaları", IsActive = true, ParentId = motorYedek.Id, CreatedDate = DateTime.UtcNow };
                
                await dbContext.Categories.AddRangeAsync(new[] { cekici, kamyon, otobus }, cancellationToken);
                addedCount += 3;
                
                // 4. seviye - Çekici Markaları
                var scania = new Category { Id = Guid.NewGuid(), Name = "Scania", Description = "Scania çekici yedek parçaları", IsActive = true, ParentId = cekici.Id, CreatedDate = DateTime.UtcNow };
                var volvo = new Category { Id = Guid.NewGuid(), Name = "Volvo", Description = "Volvo çekici yedek parçaları", IsActive = true, ParentId = cekici.Id, CreatedDate = DateTime.UtcNow };
                var mercedes = new Category { Id = Guid.NewGuid(), Name = "Mercedes", Description = "Mercedes çekici yedek parçaları", IsActive = true, ParentId = cekici.Id, CreatedDate = DateTime.UtcNow };
                var man = new Category { Id = Guid.NewGuid(), Name = "MAN", Description = "MAN çekici yedek parçaları", IsActive = true, ParentId = cekici.Id, CreatedDate = DateTime.UtcNow };
                var daf = new Category { Id = Guid.NewGuid(), Name = "DAF", Description = "DAF çekici yedek parçaları", IsActive = true, ParentId = cekici.Id, CreatedDate = DateTime.UtcNow };
                
                await dbContext.Categories.AddRangeAsync(new[] { scania, volvo, mercedes, man, daf }, cancellationToken);
                addedCount += 5;
                
                // 5. seviye - Scania Modelleri
                var scaniaModels = new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "R410", Description = "Scania R410 yedek parçaları", IsActive = true, ParentId = scania.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "R450", Description = "Scania R450 yedek parçaları", IsActive = true, ParentId = scania.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "R500", Description = "Scania R500 yedek parçaları", IsActive = true, ParentId = scania.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "S500", Description = "Scania S500 yedek parçaları", IsActive = true, ParentId = scania.Id, CreatedDate = DateTime.UtcNow }
                };
                await dbContext.Categories.AddRangeAsync(scaniaModels, cancellationToken);
                addedCount += 4;
                
                // 5. seviye - Volvo Modelleri
                var volvoModels = new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "FH16", Description = "Volvo FH16 yedek parçaları", IsActive = true, ParentId = volvo.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "FH500", Description = "Volvo FH500 yedek parçaları", IsActive = true, ParentId = volvo.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "FM", Description = "Volvo FM yedek parçaları", IsActive = true, ParentId = volvo.Id, CreatedDate = DateTime.UtcNow }
                };
                await dbContext.Categories.AddRangeAsync(volvoModels, cancellationToken);
                addedCount += 3;
                
                // 5. seviye - Mercedes Modelleri
                var mercedesModels = new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "Actros", Description = "Mercedes Actros yedek parçaları", IsActive = true, ParentId = mercedes.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Arocs", Description = "Mercedes Arocs yedek parçaları", IsActive = true, ParentId = mercedes.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Axor", Description = "Mercedes Axor yedek parçaları", IsActive = true, ParentId = mercedes.Id, CreatedDate = DateTime.UtcNow }
                };
                await dbContext.Categories.AddRangeAsync(mercedesModels, cancellationToken);
                addedCount += 3;
            }
        }
        
        // Hidrolik Pompalar kategorisini bul
        var hidrolikPompa = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Name == "Hidrolik Pompalar" && c.IsActive, cancellationToken);
            
        if (hidrolikPompa != null)
        {
            var existingDisli = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name == "Dişli Pompalar" && c.ParentId == hidrolikPompa.Id, cancellationToken);
                
            if (existingDisli == null)
            {
                var disliPompa = new Category { Id = Guid.NewGuid(), Name = "Dişli Pompalar", Description = "Dişli tip hidrolik pompalar", IsActive = true, ParentId = hidrolikPompa.Id, CreatedDate = DateTime.UtcNow };
                var pistonluPompa = new Category { Id = Guid.NewGuid(), Name = "Pistonlu Pompalar", Description = "Pistonlu tip hidrolik pompalar", IsActive = true, ParentId = hidrolikPompa.Id, CreatedDate = DateTime.UtcNow };
                var paletliPompa = new Category { Id = Guid.NewGuid(), Name = "Paletli Pompalar", Description = "Paletli tip hidrolik pompalar", IsActive = true, ParentId = hidrolikPompa.Id, CreatedDate = DateTime.UtcNow };
                
                await dbContext.Categories.AddRangeAsync(new[] { disliPompa, pistonluPompa, paletliPompa }, cancellationToken);
                addedCount += 3;
                
                // 4. seviye - Markalar
                var pompaBrands = new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "Bosch Rexroth", Description = "Bosch Rexroth dişli pompalar", IsActive = true, ParentId = disliPompa.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Parker", Description = "Parker dişli pompalar", IsActive = true, ParentId = disliPompa.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Casappa", Description = "Casappa dişli pompalar", IsActive = true, ParentId = disliPompa.Id, CreatedDate = DateTime.UtcNow }
                };
                await dbContext.Categories.AddRangeAsync(pompaBrands, cancellationToken);
                addedCount += 3;
            }
        }
        
        // PLC ve Kontrol kategorisini bul
        var plc = await dbContext.Categories
            .FirstOrDefaultAsync(c => c.Name == "PLC ve Kontrol" && c.IsActive, cancellationToken);
            
        if (plc != null)
        {
            var existingSiemens = await dbContext.Categories
                .FirstOrDefaultAsync(c => c.Name == "Siemens" && c.ParentId == plc.Id, cancellationToken);
                
            if (existingSiemens == null)
            {
                var siemens = new Category { Id = Guid.NewGuid(), Name = "Siemens", Description = "Siemens PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow };
                var omron = new Category { Id = Guid.NewGuid(), Name = "Omron", Description = "Omron PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow };
                var allenBradley = new Category { Id = Guid.NewGuid(), Name = "Allen Bradley", Description = "Allen Bradley PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow };
                var mitsubishi = new Category { Id = Guid.NewGuid(), Name = "Mitsubishi", Description = "Mitsubishi PLC sistemleri", IsActive = true, ParentId = plc.Id, CreatedDate = DateTime.UtcNow };
                
                await dbContext.Categories.AddRangeAsync(new[] { siemens, omron, allenBradley, mitsubishi }, cancellationToken);
                addedCount += 4;
                
                // 4. seviye - Siemens serileri
                var siemensModels = new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "S7-1200", Description = "Siemens S7-1200 serisi", IsActive = true, ParentId = siemens.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "S7-1500", Description = "Siemens S7-1500 serisi", IsActive = true, ParentId = siemens.Id, CreatedDate = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "S7-300", Description = "Siemens S7-300 serisi", IsActive = true, ParentId = siemens.Id, CreatedDate = DateTime.UtcNow }
                };
                await dbContext.Categories.AddRangeAsync(siemensModels, cancellationToken);
                addedCount += 3;
            }
        }
        
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return Ok(new { message = $"Derin kategoriler eklendi. Toplam {addedCount} yeni kategori oluşturuldu." });
    }
}