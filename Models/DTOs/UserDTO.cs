namespace healthmate_backend.Models.Request;

public class UserDTO
{
    public string Username { get; set; }
    public string Password { get; set; }  
    public string Type { get; set; }
    public string Email { get; set; }
    public string? ProfileImageUrl { get; set; }
}