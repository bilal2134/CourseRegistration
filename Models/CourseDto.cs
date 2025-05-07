namespace WebApplication1.Models
{
    public class CourseCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public int TeacherId { get; set; }
    }

    public class CourseUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
    }
}
