using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyIndustry.ApplicationService.Helpers;
using MyIndustry.Repository.DbContext;

namespace MyIndustry.Api.Controllers.v1;

[Route("api/v{version:apiVersion}/[controller]s")]
[ApiController]
public class LocationController : BaseController
{
    private readonly MyIndustryDbContext _context;

    public LocationController(MyIndustryDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Tüm illeri getirir
    /// </summary>
    [HttpGet("cities")]
    public async Task<IActionResult> GetCities(CancellationToken cancellationToken)
    {
        var cities = await _context.Cities
            .Where(c => c.IsActive)
            .OrderBy(c => c.PlateCode)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.PlateCode
            })
            .ToListAsync(cancellationToken);

        return Ok(new { success = true, cities });
    }

    /// <summary>
    /// Belirli bir ilin ilçelerini getirir
    /// </summary>
    [HttpGet("cities/{cityId:guid}/districts")]
    public async Task<IActionResult> GetDistricts(Guid cityId, CancellationToken cancellationToken)
    {
        var districts = await _context.Districts
            .Where(d => d.CityId == cityId && d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.CityId
            })
            .ToListAsync(cancellationToken);

        return Ok(new { success = true, districts });
    }

    /// <summary>
    /// Belirli bir ilçenin mahallelerini getirir
    /// </summary>
    [HttpGet("districts/{districtId:guid}/neighborhoods")]
    public async Task<IActionResult> GetNeighborhoods(Guid districtId, CancellationToken cancellationToken)
    {
        var neighborhoods = await _context.Neighborhoods
            .Where(n => n.DistrictId == districtId && n.IsActive)
            .OrderBy(n => n.Name)
            .Select(n => new
            {
                n.Id,
                n.Name,
                n.DistrictId
            })
            .ToListAsync(cancellationToken);

        return Ok(new { success = true, neighborhoods });
    }

    /// <summary>
    /// İl adına göre arama yapar
    /// </summary>
    [HttpGet("cities/search")]
    public async Task<IActionResult> SearchCities([FromQuery] string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Ok(new { success = true, cities = Array.Empty<object>() });
        }

        var normalizedQuery = SearchTermHelper.NormalizeForSearch(query);
        var candidates = await _context.Cities
            .Where(c => c.IsActive)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var cities = candidates
            .Where(c => SearchTermHelper.NormalizeForSearch(c.Name).Contains(normalizedQuery))
            .OrderBy(c => c.Name)
            .Take(10)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.PlateCode
            })
            .ToList();

        return Ok(new { success = true, cities });
    }

    /// <summary>
    /// İlçe adına göre arama yapar
    /// </summary>
    [HttpGet("districts/search")]
    public async Task<IActionResult> SearchDistricts([FromQuery] string query, [FromQuery] Guid? cityId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Ok(new { success = true, districts = Array.Empty<object>() });
        }

        var normalizedQuery = SearchTermHelper.NormalizeForSearch(query);
        var queryable = _context.Districts.Where(d => d.IsActive);

        if (cityId.HasValue)
        {
            queryable = queryable.Where(d => d.CityId == cityId.Value);
        }

        var candidates = await queryable
            .Include(d => d.City)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var districts = candidates
            .Where(d => SearchTermHelper.NormalizeForSearch(d.Name).Contains(normalizedQuery))
            .OrderBy(d => d.Name)
            .Take(20)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.CityId,
                CityName = d.City.Name
            })
            .ToList();

        return Ok(new { success = true, districts });
    }

    /// <summary>
    /// Mahalle adına göre arama yapar
    /// </summary>
    [HttpGet("neighborhoods/search")]
    public async Task<IActionResult> SearchNeighborhoods([FromQuery] string query, [FromQuery] Guid? districtId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Ok(new { success = true, neighborhoods = Array.Empty<object>() });
        }

        var normalizedQuery = SearchTermHelper.NormalizeForSearch(query);
        var queryable = _context.Neighborhoods.Where(n => n.IsActive);

        if (districtId.HasValue)
        {
            queryable = queryable.Where(n => n.DistrictId == districtId.Value);
        }
        else if (!IsInMemoryDatabase())
        {
            queryable = queryable.Where(n =>
                EF.Functions.ILike(n.Name, $"%{query}%") ||
                EF.Functions.ILike(n.Name, $"%{normalizedQuery}%"));
        }

        var candidates = await queryable
            .Include(n => n.District)
            .ThenInclude(d => d.City)
            .AsNoTracking()
            .Take(districtId.HasValue ? 10_000 : 500)
            .ToListAsync(cancellationToken);

        var neighborhoods = candidates
            .Where(n => SearchTermHelper.NormalizeForSearch(n.Name).Contains(normalizedQuery))
            .OrderBy(n => n.Name)
            .Take(30)
            .Select(n => new
            {
                n.Id,
                n.Name,
                n.DistrictId,
                DistrictName = n.District.Name,
                CityName = n.District.City.Name
            })
            .ToList();

        return Ok(new { success = true, neighborhoods });
    }

    private bool IsInMemoryDatabase() =>
        _context.Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true;
}
