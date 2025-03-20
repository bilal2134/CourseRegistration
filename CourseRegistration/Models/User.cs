using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseRegistration.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        // For simplicity, we add a flag to differentiate between normal users and admins.
        public bool IsAdmin { get; set; } = false;

        // Navigation property
        public ICollection<CourseRegistration> CourseRegistrations { get; set; }
    }
}
