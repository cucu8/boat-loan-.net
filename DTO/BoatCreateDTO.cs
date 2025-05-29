using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SadikTuranECommerce.DTO
{
    public class BoatCreateDTO
    {
        [Required(ErrorMessage = "Boat name is required.")]
        [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description can't be longer than 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price per hour is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal PricePerHour { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 1000, ErrorMessage = "Capacity must be at least 1.")]
        public int Capacity { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "AvailableFrom date is required.")]
        public DateTime AvailableFrom { get; set; }

        [Required(ErrorMessage = "AvailableTo date is required.")]
        public DateTime AvailableTo { get; set; }

        [Required(ErrorMessage = "Owner ID is required.")]
        public int OwnerId { get; set; }

        [Required(ErrorMessage = "District ID is required.")]
        public int DistrictId { get; set; }

        public List<IFormFile>? Images { get; set; }
    }
}
