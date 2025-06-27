using System.ComponentModel.DataAnnotations.Schema;

namespace healthmate_backend.Models
{
    [Table("admins")]
    public class Admin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
} 