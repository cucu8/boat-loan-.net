using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadikTuranECommerce.DTO;
using SadikTuranECommerce.Entities;
using static System.Net.Mime.MediaTypeNames;

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
                .ThenInclude(b => b.City)
                .ThenInclude(b => b.Country)
                .Include(b => b.Images)
                .Select(b => new BoatResponseDTO
                {
                    Id = b.Id,
                    Name = b.Name,
                    Description = b.Description,
                    PricePerHour = b.PricePerHour,
                    Capacity = b.Capacity,
                    IsAvailable = b.IsAvailable,
                    OwnerName = b.Name,
                    DistrictName = b.District.Name,
                    DistrictId = b.DistrictId,
                    CityId = b.District.CityId,
                    CityName = b.District.City.Name,
                    CountryId = b.District.City.CountryId,
                    CountryName = b.District.City.Country.Name,
                    Images = b.Images.Select(img => new BoatImageDTO
                    {
                        Id = img.Id,
                        Base64Image = $"data:image/jpeg;base64,{Convert.ToBase64String(img.ImageData)}"
                    }).ToList(),
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
                    .ThenInclude(b => b.City)
                    .ThenInclude(b => b.Country)
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
                DistrictId = b.DistrictId,
                CityId = b.District.CityId,
                CityName = b.District.City.Name,
                CountryId = b.District.City.CountryId,
                CountryName = b.District.City.Country.Name,
                Images = b.Images.Select(img => new BoatImageDTO
                {
                    Id = img.Id,
                    Base64Image = $"data:image/jpeg;base64,{Convert.ToBase64String(img.ImageData)}"
                }).ToList(),
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
                    .ThenInclude(d => d.City)
                    .ThenInclude(c => c.Country)
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
                OwnerName = boat.Name, // User entity'de Name varsa
                DistrictName = boat.District.Name,
                DistrictId = boat.DistrictId,
                CityId = boat.District.CityId,
                CityName = boat.District.City.Name,
                CountryId = boat.District.City.CountryId,
                CountryName = boat.District.City.Country.Name,
                Images = boat.Images.Select(img => new BoatImageDTO
                {
                    Id = img.Id,
                    Base64Image = $"data:image/jpeg;base64,{Convert.ToBase64String(img.ImageData)}"
                }).ToList(),
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



            if (request.Images != null)
            {
                foreach (var formFile in request.Images)
                {
                    if (formFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await formFile.CopyToAsync(memoryStream);
                            var imageData = memoryStream.ToArray();

                            boat.Images.Add(new BoatImage
                            {
                                ImageData = imageData
                            });
                        }
                    }
                }
            }

            _context.Boats.Add(boat);
            await _context.SaveChangesAsync();

            var createdBoat = await _context.Boats
                .Include(b => b.Owner)
                .Include(b => b.District)
                .ThenInclude(b => b.City)
                .ThenInclude(b => b.Country)
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
                DistrictId = createdBoat.DistrictId,
                CityId = createdBoat.District.CityId,
                CityName = createdBoat.District.City.Name,
                CountryId = createdBoat.District.City.CountryId,
                CountryName = createdBoat.District.City.Country.Name,
                OwnerName = createdBoat.Owner.Email,
                Images = createdBoat.Images.Select(img => new BoatImageDTO
                {
                    Id = img.Id,
                    Base64Image = $"data:image/jpeg;base64,{Convert.ToBase64String(img.ImageData)}"
                }).ToList()
            };

            return CreatedAtAction(nameof(GetBoat), new { id = boat.Id }, dto);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBoat(int id, [FromForm] BoatEditDTO request)
        {
            var boat = await _context.Boats
                .Include(b => b.Images) // Mevcut resimleri çek
                .FirstOrDefaultAsync(b => b.Id == id);

            if (boat == null)
            {
                return NotFound($"Boat with ID {id} not found.");
            }

            // 1. Tekne bilgilerini güncelle
            boat.Name = request.Name;
            boat.Description = request.Description;
            boat.PricePerHour = request.PricePerHour;
            boat.Capacity = request.Capacity;
            boat.IsAvailable = request.IsAvailable;
            boat.AvailableFrom = request.AvailableFrom;
            boat.AvailableTo = request.AvailableTo;
            boat.DistrictId = request.DistrictId;
            // boat.OwnerId = request.OwnerId; // OwnerId genellikle update edilmez, sabit kalır. Gerekiyorsa açılabilir.

            // 2. Mevcut resimleri silme
            if (request.ImagesToDelete != null && request.ImagesToDelete.Any())
            {
                // Silinecek resim ID'lerine sahip kayıtları bul ve sil
                var imagesToRemove = boat.Images
                                         .Where(img => request.ImagesToDelete.Contains(img.Id))
                                         .ToList();

                foreach (var image in imagesToRemove)
                {
                    // Base64 olduğu için fiziksel dosya silme yok, sadece veritabanı kaydını siliyoruz.
                    _context.BoatImages.Remove(image);
                }
            }

            // 3. Yeni resimleri ekle (Base64 olarak)
            if (request.NewImages != null && request.NewImages.Any())
            {
                foreach (var formFile in request.NewImages)
                {
                    if (formFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await formFile.CopyToAsync(memoryStream);
                            var imageData = memoryStream.ToArray();

                            boat.Images.Add(new BoatImage
                            {
                                BoatId = boat.Id, // Hangi tekneye ait olduğunu belirt
                                ImageData = imageData
                            });
                        }
                    }
                }
            }

            await _context.SaveChangesAsync(); // Tüm değişiklikleri kaydet

            // Güncellenmiş tekneyi ve resimlerini döndürmek isteyebiliriz
            var updatedBoat = await _context.Boats
                .Include(b => b.Owner)
                .Include(b => b.District)
                .ThenInclude(d => d.City)
                .ThenInclude(c => c.Country)
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == id);

            var updatedDto = new BoatResponseDTO
            {
                Id = updatedBoat.Id,
                Name = updatedBoat.Name,
                Description = updatedBoat.Description,
                PricePerHour = updatedBoat.PricePerHour,
                Capacity = updatedBoat.Capacity,
                IsAvailable = updatedBoat.IsAvailable,
                DistrictName = updatedBoat.District.Name,
                DistrictId = updatedBoat.DistrictId,
                CityId = updatedBoat.District.CityId,
                CityName = updatedBoat.District.City.Name,
                CountryId = updatedBoat.District.City.CountryId,
                CountryName = updatedBoat.District.City.Country.Name,
                OwnerName = updatedBoat.Owner.Email, // Owner entity'nizdeki doğru alan adını kullanın
                Images = updatedBoat.Images.Select(img => new BoatImageDTO
                {
                    Id = img.Id,
                    Base64Image = img.ImageData != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(img.ImageData)}" : null
                }).ToList(),
                OwnerPhoneNumber = updatedBoat.Owner.PhoneNumber
            };

            return Ok(updatedDto); // 200 OK ile güncellenmiş DTO'yu döndür
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

            _context.Boats.Remove(boat);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Boat deleted successfully." });
        }
    }
}
