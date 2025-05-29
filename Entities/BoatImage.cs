namespace SadikTuranECommerce.Entities
{
    public class BoatImage
    {
        public int Id { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public int BoatId { get; set; }

        public Boat Boat { get; set; } = null!;
    }
}
