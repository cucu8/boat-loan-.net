using BoatProject.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SadikTuranECommerce.DTO;
using SadikTuranECommerce.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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

        private BoatResponseDTO MapBoatToResponseDTO(Boat boat)
        {
            if (boat == null)
            {
                return null; 
            }

            return new BoatResponseDTO
            {
                Id = boat.Id,
                Name = boat.Name,
                Description = boat.Description,
                PricePerHour = boat.PricePerHour,
                Capacity = boat.Capacity,
                IsAvailable = boat.IsAvailable,
                OwnerName = boat.Owner?.Email, // Use null-conditional operator for safety
                OwnerPhoneNumber = boat.Owner?.PhoneNumber, // Use null-conditional operator for safety
                DistrictName = boat.District?.Name,
                DistrictId = boat.DistrictId,
                CityId = boat.District.CityId,
                CityName = boat.District.City.Name,
                CountryId = boat.District.City.CountryId,
                CountryName = boat.District.City?.Country?.Name,
                Images = boat.Images?.Select(img => new BoatImageDTO
                {
                    Id = img.Id,
                    Base64Image = img.ImageData != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(img.ImageData)}" : null
                }).ToList() ?? new List<BoatImageDTO>() // Handle null Images collection
            };
        }

        private bool IsValidateImage(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var contentType = file.ContentType.ToLowerInvariant();

            return ImageValidationConstants.AllowedImageExtensions.Contains(extension) &&
                   ImageValidationConstants.AllowedImageContentTypes.Contains(contentType);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoatResponseDTO>>> GetAllBoats([FromQuery] int? capacity, int? price, string city, string disctrict)
        {
            IQueryable<Boat> query = _context.Boats
               .Include(b => b.Owner)
               .Include(b => b.District)
               .ThenInclude(d => d.City)
               .ThenInclude(c => c.Country)
               .Include(b => b.Images);

            // Apply capacity filter if provided
            if (capacity.HasValue)
            {
                query = query.Where(b => b.Capacity >= capacity.Value);
            }

            // Apply price filter if provided
            if (price.HasValue)
            {
                query = query.Where(b => b.PricePerHour <= price.Value); 
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(b => b.District.City.Name.Contains(city));
            }

            // Apply district filter if provided
            if (!string.IsNullOrWhiteSpace(disctrict))
            {
                query = query.Where(b => b.District.Name.Contains(disctrict));
            }


            var boats = await query.ToListAsync();

            var boatDtos = boats.Select(MapBoatToResponseDTO).ToList();

            return Ok(boatDtos);
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

            var boatDtos = boats.Select(MapBoatToResponseDTO).ToList(); // Use the new mapping method

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

            var dto = MapBoatToResponseDTO(boat);
          
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

                if (request.Images.Count > 5)
                {
                    return BadRequest(new { Message = "You can upload a maximum of 5 images." });
                }

                foreach (var formFile in request.Images)
                {
                    if (formFile.Length > 0)
                    {

                        if (!IsValidateImage(formFile))
                        {
                            return BadRequest(new
                            {
                                Message = $"Invalid file type: {formFile.FileName}. Only JPG, JPEG, PNG, WEBP, HEIC, HEIF files are allowed."
                            });
                        }

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

                var dto = MapBoatToResponseDTO(boat);

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

            // 2. Silinecek görselleri belirle
            var existingImageCount = boat.Images.Count;
            var deleteCount = request.ImagesToDelete?.Count ?? 0;
            var newImageCount = request.NewImages?.Count ?? 0;

            // 🔒 Toplam resim 5'i geçemez (mevcut - silinecek + eklenecek)
            var remainingImageCount = existingImageCount - deleteCount + newImageCount;
            if (remainingImageCount > 5)
            {
                return BadRequest(new { Message = "En fazla 5 resim yükleyebilirsiniz" });
            }

            // 3. Yeni resimleri ekle (Base64 olarak)
            if (request.NewImages != null && request.NewImages.Any())
            {
                foreach (var formFile in request.NewImages)
                {
                    if (formFile.Length > 0)
                    {

                        if (!IsValidateImage(formFile))
                        {
                            return BadRequest(new
                            {
                                Message = $"Invalid file type: {formFile.FileName}. Only JPG, JPEG, PNG, WEBP, HEIC, HEIF files are allowed."
                            });
                        }

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

            // Güncellenmiş tekneyi ve resimlerini döndür
            var updatedBoat = await _context.Boats
                .Include(b => b.Owner)
                .Include(b => b.District)
                .ThenInclude(d => d.City)
                .ThenInclude(c => c.Country)
                .Include(b => b.Images)
                .FirstOrDefaultAsync(b => b.Id == id);

            var updatedDto = MapBoatToResponseDTO(updatedBoat);

            return Ok(updatedDto);
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
