using SadikTuranECommerce.Entities;

public class Boat
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal PricePerHour { get; set; }

    public int Capacity { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime AvailableFrom { get; set; }

    public DateTime AvailableTo { get; set; }

    public int OwnerId { get; set; }

    public User Owner { get; set; } = null!;

    public ICollection<BoatImage> Images { get; set; } = new List<BoatImage>();

    public int DistrictId { get; set; }

    public District District { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
