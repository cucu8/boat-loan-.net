namespace BoatProject.Constants
{
    public class ImageValidationConstants
    {
        public static readonly string[] AllowedImageExtensions = {
            ".jpg", ".jpeg", ".png", ".gif", ".webp", ".heic", ".heif"
        };

        // Geçerli MIME içerik türleri
        public static readonly string[] AllowedImageContentTypes = {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "image/heic",
            "image/heif"
        };
    }
}
