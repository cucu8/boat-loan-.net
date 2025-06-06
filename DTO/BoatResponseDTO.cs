using SadikTuranECommerce.Entities;

namespace SadikTuranECommerce.DTO
{
    public class BoatResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PricePerHour { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailable { get; set; }
        public string OwnerName { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public string OwnerPhoneNumber { get; set; }
        public List<BoatImageDTO> Images { get; set; }

    }
}
