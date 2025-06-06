namespace SadikTuranECommerce.Entities
{
    public class BoatImage
    {
        public int Id { get; set; }

        public byte[] ImageData { get; set; } 

        public int BoatId { get; set; }

        public Boat Boat { get; set; } = null!;
    }
}
