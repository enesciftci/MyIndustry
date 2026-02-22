using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        var cities = await _context.Cities
            .Where(c => c.IsActive && c.Name.ToLower().Contains(query.ToLower()))
            .OrderBy(c => c.Name)
            .Take(10)
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
    /// İlçe adına göre arama yapar
    /// </summary>
    [HttpGet("districts/search")]
    public async Task<IActionResult> SearchDistricts([FromQuery] string query, [FromQuery] Guid? cityId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return Ok(new { success = true, districts = Array.Empty<object>() });
        }

        var queryable = _context.Districts
            .Where(d => d.IsActive && d.Name.ToLower().Contains(query.ToLower()));

        if (cityId.HasValue)
        {
            queryable = queryable.Where(d => d.CityId == cityId.Value);
        }

        var districts = await queryable
            .OrderBy(d => d.Name)
            .Take(20)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.CityId,
                CityName = d.City.Name
            })
            .ToListAsync(cancellationToken);

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

        var queryable = _context.Neighborhoods
            .Where(n => n.IsActive && n.Name.ToLower().Contains(query.ToLower()));

        if (districtId.HasValue)
        {
            queryable = queryable.Where(n => n.DistrictId == districtId.Value);
        }

        var neighborhoods = await queryable
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
            .ToListAsync(cancellationToken);

        return Ok(new { success = true, neighborhoods });
    }
}
