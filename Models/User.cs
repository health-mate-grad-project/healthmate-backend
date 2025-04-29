namespace healthmate_backend.Models
{
    public abstract class User
    {
        public int Id { get; set; }
    	public string Username { get; set; }
    	public string Password { get; set; }
    	public string Email { get; set; }
    	public string Type { get; set; }
    }
}