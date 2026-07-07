using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyIndustry.Container.Logging;
using MyIndustry.Repository.Repository;
using System.Text;
using System.Xml.Linq;

namespace MyIndustry.Api.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class SEOController : BaseController
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IGenericRepository<Domain.Aggregate.Category> _categoryRepository;
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SEOController> _logger;

    public SEOController(
        IGenericRepository<Domain.Aggregate.Service> serviceRepository,
        IGenericRepository<Domain.Aggregate.Category> categoryRepository,
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository,
        IUnitOfWork unitOfWork,
        ILogger<SEOController> logger)
    {
        _serviceRepository = serviceRepository;
        _categoryRepository = categoryRepository;
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("sitemap.xml")]
    public async Task<IActionResult> GetSitemap(CancellationToken cancellationToken)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var currentDate = DateTime.UtcNow;
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";

        XElement CreateUrlElement(string loc, string lastmod, string changefreq, string priority) =>
            new(ns + "url",
                new XElement(ns + "loc", loc),
                new XElement(ns + "lastmod", lastmod),
                new XElement(ns + "changefreq", changefreq),
                new XElement(ns + "priority", priority));

        var sitemap = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement(ns + "urlset",
                new XAttribute(XNamespace.Xmlns + "xsi", xsi.NamespaceName),
                new XAttribute(xsi + "schemaLocation", $"{ns.NamespaceName} {ns.NamespaceName}/sitemap.xsd"),
                CreateUrlElement(baseUrl, currentDate.ToString("yyyy-MM-dd"), "daily", "1.0"),
                CreateUrlElement($"{baseUrl}/services/filter", currentDate.ToString("yyyy-MM-dd"), "daily", "0.9"),
                CreateUrlElement($"{baseUrl}/sellers", currentDate.ToString("yyyy-MM-dd"), "daily", "0.8")
            )
        );

        // Add all active and approved services
        var services = await _serviceRepository
            .GetAllQuery()
            .Where(s => s.IsActive && s.IsApproved)
            .Select(s => new { s.Id, s.ModifiedDate, s.CreatedDate, s.Slug })
            .ToListAsync(cancellationToken);

        foreach (var service in services)
        {
            var lastmod = service.ModifiedDate.HasValue 
                ? service.ModifiedDate.Value 
                : service.CreatedDate;
            var url = string.IsNullOrEmpty(service.Slug)
                ? $"{baseUrl}/services/{service.Id}"
                : $"{baseUrl}/services/{service.Slug}";

            sitemap.Root?.Add(
                CreateUrlElement(url, lastmod.ToString("yyyy-MM-dd"), "weekly", "0.7")
            );
        }

        // Add all active categories
        var categories = await _categoryRepository
            .GetAllQuery()
            .Where(c => c.IsActive)
            .Select(c => new { c.Id, c.Slug })
            .ToListAsync(cancellationToken);

        foreach (var category in categories)
        {
            var url = string.IsNullOrEmpty(category.Slug)
                ? $"{baseUrl}/services/filter?categoryId={category.Id}"
                : $"{baseUrl}/kategori/{category.Slug}";

            sitemap.Root?.Add(
                CreateUrlElement(url, currentDate.ToString("yyyy-MM-dd"), "weekly", "0.6")
            );
        }

        // Add all active sellers
        var sellers = await _sellerRepository
            .GetAllQuery()
            .Where(s => s.IsActive)
            .Select(s => new { s.Id })
            .ToListAsync(cancellationToken);

        foreach (var seller in sellers)
        {
            sitemap.Root?.Add(
                CreateUrlElement($"{baseUrl}/seller/{seller.Id}", currentDate.ToString("yyyy-MM-dd"), "weekly", "0.6")
            );
        }

        return Content(sitemap.ToString(), "application/xml", Encoding.UTF8);
    }

    [HttpGet("robots.txt")]
    public IActionResult GetRobotsTxt()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        var robotsTxt = $@"User-agent: *
Allow: /
Disallow: /api/
Disallow: /admin/
Disallow: /seller/
Disallow: /login
Disallow: /register
Disallow: /profile
Disallow: /messages
Disallow: /favorites
Disallow: /support

# Sitemap
Sitemap: {baseUrl}/api/v1/seo/sitemap.xml

# Crawl-delay
Crawl-delay: 1
";

        return Content(robotsTxt, "text/plain", Encoding.UTF8);
    }

    [HttpPost("generate-slugs")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GenerateSlugsForExistingServices(CancellationToken cancellationToken)
    {
        var services = await _serviceRepository
            .GetAllQuery()
            .Where(s => string.IsNullOrEmpty(s.Slug))
            .ToListAsync(cancellationToken);

        var updatedCount = 0;
        foreach (var service in services)
        {
            var baseSlug = ApplicationService.Helpers.SlugHelper.GenerateSlug(service.Title);
            var uniqueSlug = await ApplicationService.Helpers.SlugHelper.GenerateUniqueSlugAsync(
                baseSlug,
                async (slug) => await _serviceRepository
                    .GetAllQuery()
                    .AnyAsync(s => s.Slug == slug && s.Id != service.Id, cancellationToken)
            );
            
            service.Slug = uniqueSlug;
            
            // Also generate meta fields if empty
            if (string.IsNullOrEmpty(service.MetaTitle))
            {
                var listingTypeText = service.ListingType == Domain.Aggregate.ValueObjects.ListingType.ForSale ? "Satılık" : "Kiralık";
                service.MetaTitle = $"{service.Title} - {listingTypeText} | MyIndustry";
            }
            
            if (string.IsNullOrEmpty(service.MetaDescription))
            {
                service.MetaDescription = service.Description.Length > 160 
                    ? service.Description.Substring(0, 157) + "..." 
                    : service.Description;
            }
            
            _serviceRepository.Update(service);
            updatedCount++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        AdminAuditLogger.LogAdminAction(_logger, "GenerateSlugs", GetUserId().ToString(), null,
            HttpContext.Items[CorrelationIdConstants.ItemKey]?.ToString(), new { updatedCount });

        return Ok(new { message = $"{updatedCount} ilan için slug oluşturuldu.", updatedCount });
    }
}
