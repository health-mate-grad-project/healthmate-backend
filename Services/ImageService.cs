using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

public class ImageService
{
    private readonly Cloudinary _cloudinary;

    public ImageService(IConfiguration config)
    {
        var cloudinaryUrl = config["CLOUDINARY_URL"];

        var uri = new Uri(cloudinaryUrl.Replace("cloudinary://", "https://"));
        var parts = uri.UserInfo.Split(':');

        var account = new Account(
            uri.Host,          // Cloud name
            parts[0],          // API key
            parts[1]           // API secret
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "healthmate/profiles"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl.ToString();
    }
}