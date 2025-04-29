using System.Collections.Generic;
using healthmate_backend.Models;

namespace healthmate_backend.Models
{
    public class Clinic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        // ➡️ Add relationship
        public ICollection<Doctor> Doctors { get; set; }
    }
}