namespace healthmate_backend.Models
{
    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string Role { get; set; }
        public required string Email { get; set; }
        public bool AcceptedTerms { get; set; }
    }
}