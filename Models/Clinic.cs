using System.Collections.Generic;
using healthmate_backend.Models;

namespace healthmate_backend.Models
{
    public class Clinic
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Location { get; set; }
        // ➡️ Add relationship
        public required ICollection<Doctor> Doctors { get; set; }
    }
}