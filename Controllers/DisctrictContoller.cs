using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadikTuranECommerce.DTO;

namespace SadikTuranECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DisctrictContoller: ControllerBase
    {

        private readonly BoatRentalDbContext _context;

        public DisctrictContoller(BoatRentalDbContext context)
        {
            _context = context;
        }

        [HttpGet("{cityId}")]
        public async Task<ActionResult<IEnumerable<DiscrictDTO>>> GetCitiesByCountry(int cityId)
        {
            var cities = await _context.Districts
                .Where(c => c.CityId == cityId)
                .Select(c => new DiscrictDTO
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(cities);
        }

    }
}
