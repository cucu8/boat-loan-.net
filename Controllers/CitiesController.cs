using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadikTuranECommerce.DTO;

namespace SadikTuranECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitiesController : ControllerBase
    {

        private readonly BoatRentalDbContext _context;

        public CitiesController(BoatRentalDbContext context)
        {
            _context = context;
        }

        [HttpGet("{countryId}")]
        public async Task<ActionResult<IEnumerable<CityDTO>>> GetCitiesByCountry(int countryId)
        {
            var cities = await _context.Cities
                .Where(c => c.CountryId == countryId)
                .Select(c => new CityDTO
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(cities);
        }


    }
}
