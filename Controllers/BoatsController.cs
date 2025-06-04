using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadikTuranECommerce.DTO;
using SadikTuranECommerce.Entities;

namespace SadikTuranECommerce.Controllers
{

    [Route("api/boats")]
    public class BoatsController : ControllerBase
    {
        private readonly BoatRentalDbContext _context;

        public BoatsController(BoatRentalDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoatResponseDTO>>> GetAllBoats()
        {
            var boats = await _context.Boats
                .Include(b => b.Owner)
                .Include(b => b.District)
                .Include(b => b.Images)
                .Select(b => new BoatResponseDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    PricePerHour = b.PricePerHour,
                    Capacity = b.Capacity,
                    IsAvailable = b.IsAvailable,
                    OwnerName = b.Name, // User entity'de Name varsa
                    DistrictName = b.District.Name,
                    DistrictId= b.DistrictId,
                    CityId = b.District.CityId,
                    CityName = b.District.City.Name,
                    CountryId = b.District.City.CountryId,
                    CountryName = b.District.City.Country.Name,
                    ImageUrls = b.Images.Select(img => img.ImageUrl).ToList(),
                    OwnerPhoneNumber = b.Owner.PhoneNumber
                })
                .ToListAsync();

            return Ok(boats);
        }

        // GET: api/boats/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Boat>>> GetBoatsByUser(int userId)
        {
            var boats = await _context.Boats.Where(b => b.OwnerId == userId)
                    .Include(b => b.Images)
                    .Include(b => b.District)
                    .Include(b => b.Owner)
                    .ToListAsync();

            var boatDtos = boats.Select(b => new BoatResponseDTO
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                PricePerHour = b.PricePerHour,
                Capacity = b.Capacity,
                IsAvailable = b.IsAvailable,
                OwnerName = b.Name, // User entity'de Name varsa
                DistrictName = b.District.Name,
                ImageUrls = b.Images.Select(img => img.ImageUrl).ToList(),
                OwnerPhoneNumber = b.Owner.PhoneNumber
            });

            return Ok(boatDtos);
        }

        // GET: api/boats/3
        [HttpGet("{id}")]
        public async Task<ActionResult<BoatResponseDTO>> GetBoat(int id)
        {
            var boat = await _context.Boats
                    .Include(b => b.Images)
                    .Include(b => b.District)
                    .Include(b => b.Owner)
                    .FirstOrDefaultAsync(b => b.Id == id);

            if (boat == null)
                return NotFound();

            var dto = new BoatResponseDTO
            {
                Id = boat.Id,
                Name = boat.Name,
                Description = boat.Description,
                PricePerHour = boat.PricePerHour,
                Capacity = boat.Capacity,
                IsAvailable = boat.IsAvailable,
                DistrictName = boat.District.ToString(),
                OwnerName = boat.Owner.ToString(),
                ImageUrls = boat.Images.Select(img => img.ImageUrl).ToList(),
                OwnerPhoneNumber = boat.Owner.PhoneNumber
            };

            return Ok(dto);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBoat([FromForm] BoatCreateDTO request)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.OwnerId);
            if (!userExists)
            {
                return BadRequest($"User with ID {request.OwnerId} does not exist.");
            }

            var boat = new Boat
            {
                Name = request.Name,
                Description = request.Description,
                PricePerHour = request.PricePerHour,
                Capacity = request.Capacity,
                IsAvailable = request.IsAvailable,
                AvailableFrom = request.AvailableFrom,
                AvailableTo = request.AvailableTo,
                OwnerId = request.OwnerId,
                DistrictId = request.DistrictId,
                Images = new List<BoatImage>()
            };

            // wwwroot/uploads dizini
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            if (request.Images != null)
            {
                foreach (var formFile in request.Images)
                {
                    if (formFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await formFile.CopyToAsync(stream);
                        }

                        var imageUrl = $"/uploads/{fileName}";

                        boat.Images.Add(new BoatImage
                        {
                            ImageUrl = imageUrl
                        });
                    }
                }
            }

            _context.Boats.Add(boat);
            await _context.SaveChangesAsync();

            var createdBoat = await _context.Boats
    .Include(b => b.Owner)
    .Include(b => b.District)
    .Include(b => b.Images)
    .FirstOrDefaultAsync(b => b.Id == boat.Id);

            var dto = new BoatResponseDTO
            {
                Id = createdBoat.Id,
                Name = createdBoat.Name,
                Description = createdBoat.Description,
                PricePerHour = createdBoat.PricePerHour,
                Capacity = createdBoat.Capacity,
                IsAvailable = createdBoat.IsAvailable,
                DistrictName = createdBoat.District.Name,
                OwnerName = createdBoat.Owner.Email,
                ImageUrls = createdBoat.Images.Select(img => img.ImageUrl).ToList()
            };

            return CreatedAtAction(nameof(GetBoat), new { id = boat.Id }, dto);
        }



        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBoat(int id)
        {
            var boat = await _context.Boats
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (boat == null)
                return NotFound(new { Message = $"Boat with ID {id} not found." });

            // Resimleri wwwroot/uploads klasöründen sil
            foreach (var image in boat.Images)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Boats.Remove(boat);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Boat deleted successfully." });
        }
    }
}
