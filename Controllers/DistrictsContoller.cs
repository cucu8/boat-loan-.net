using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadikTuranECommerce.DTO;

namespace SadikTuranECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistrictsController : ControllerBase
    {

        private readonly BoatRentalDbContext _context;

        public DistrictsController(BoatRentalDbContext context)
        {
            _context = context;
        }

        [HttpGet("{cityId}")]
        public async Task<ActionResult<IEnumerable<DiscrictDTO>>> GetCitiesByCountry(int cityId)
        {
            var discricts = await _context.Districts
                .Where(c => c.CityId == cityId)
                .Select(c => new DiscrictDTO
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(discricts);
        }

    }
}
