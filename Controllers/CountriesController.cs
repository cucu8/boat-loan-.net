using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadikTuranECommerce.DTO;
using SadikTuranECommerce.Entities;
using System;

namespace SadikTuranECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class CountriesController : ControllerBase
    {
     
        private readonly BoatRentalDbContext _context;

        public CountriesController(BoatRentalDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CountryDTO>>> GetCountries()
        {
            var countries = await _context.Countries
                .Select(c => new CountryDTO
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(countries);
        }

    }
}
