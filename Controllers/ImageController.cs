using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using healthmate_backend.Models;
using healthmate_backend.Services;

namespace DefaultNamespace;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ImageService _imageService;

    public ImageController(ImageService imageService, AppDbContext context)
    {
        _imageService = imageService;
        _context = context;
    }

    [Authorize]
    [HttpPost("upload-profile-image")]
    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { message = "Invalid token" });

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound(new { message = "User not found" });

        var imageUrl = await _imageService.UploadImageAsync(file);
        user.ProfileImageUrl = imageUrl;

        await _context.SaveChangesAsync();
        return Ok(new { imageUrl });
    }
}