using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CourseRegistration.Models
{
    public class Course
    {
        public Course()
        {
            CourseRegistrations = new List<CourseRegistration>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Instructor { get; set; }

        // Navigation property
        public ICollection<CourseRegistration> CourseRegistrations { get; set; }
    }
}
