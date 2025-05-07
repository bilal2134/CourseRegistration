using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseRepository _courseRepo;
        private readonly ITeacherRepository _teacherRepo;
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly IStudentRepository _studentRepo;

        public CourseController(ICourseRepository courseRepo, ITeacherRepository teacherRepo, IEnrollmentRepository enrollmentRepo, IStudentRepository studentRepo)
        {
            _courseRepo = courseRepo;
            _teacherRepo = teacherRepo;
            _enrollmentRepo = enrollmentRepo;
            _studentRepo = studentRepo;
        }

        // TEACHER: Create a course
        [HttpPost("create")]
        public async Task<IActionResult> CreateCourse([FromBody] CourseCreateDto dto)
        {
            var teacher = await _teacherRepo.GetByIdAsync(dto.TeacherId);
            if (teacher == null) return BadRequest("Invalid teacher.");
            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                Capacity = dto.Capacity,
                TeacherId = dto.TeacherId,
                Enrollments = new List<Enrollment>()
            };
            await _courseRepo.AddAsync(course);
            await _courseRepo.SaveAsync();
            // Return a DTO, not the entity, to avoid cycles
            return Ok(new {
                course.Id,
                course.Title,
                course.Description,
                course.Capacity,
                course.TeacherId
            });
        }

        // TEACHER: Update a course
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseUpdateDto dto)
        {
            var course = await _courseRepo.GetByIdAsync(id);
            if (course == null) return NotFound();
            course.Title = dto.Title;
            course.Description = dto.Description;
            course.Capacity = dto.Capacity;
            await _courseRepo.SaveAsync();
            // Return a DTO, not the entity, to avoid cycles
            return Ok(new {
                course.Id,
                course.Title,
                course.Description,
                course.Capacity,
                course.TeacherId
            });
        }

        // TEACHER: Delete a course
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _courseRepo.GetByIdAsync(id);
            if (course == null) return NotFound();
            _courseRepo.Delete(course);
            await _courseRepo.SaveAsync();
            return Ok("Course deleted.");
        }

        // TEACHER: Get students enrolled in a course
        [HttpGet("{courseId}/students")]
        public async Task<IActionResult> GetStudentsInCourse(int courseId)
        {
            var students = await _courseRepo.GetStudentsByCourseIdAsync(courseId);
            var count = await _enrollmentRepo.GetEnrollmentCountByCourseIdAsync(courseId);
            return Ok(new { count, students });
        }

        // TEACHER: Get courses for a specific teacher
        [HttpGet("teacher/{teacherId}")]
        public async Task<IActionResult> GetCoursesByTeacher(int teacherId)
        {
            var courses = await _courseRepo.GetCoursesByTeacherIdAsync(teacherId);
            var result = new List<object>();
            foreach (var c in courses)
            {
                var enrolledCount = await _enrollmentRepo.GetEnrollmentCountByCourseIdAsync(c.Id);
                result.Add(new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    c.Capacity,
                    c.TeacherId,
                    EnrolledCount = enrolledCount
                });
            }
            return Ok(result);
        }

        // STUDENT: Get all available courses
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _courseRepo.GetCoursesWithTeacherAsync();
            var result = new List<object>();
            foreach (var c in courses)
            {
                var enrolledCount = await _enrollmentRepo.GetEnrollmentCountByCourseIdAsync(c.Id);
                result.Add(new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    c.Capacity,
                    TeacherName = c.Teacher?.Name,
                    EnrolledCount = enrolledCount
                });
            }
            return Ok(result);
        }

        // STUDENT: Enroll in a course
        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollmentRequest req)
        {
            var course = await _courseRepo.GetByIdAsync(req.CourseId);
            if (course == null) return NotFound("Course not found.");
            var student = await _studentRepo.GetByIdAsync(req.StudentId);
            if (student == null) return NotFound("Student not found.");
            var enrolledCount = await _enrollmentRepo.GetEnrollmentCountByCourseIdAsync(req.CourseId);
            if (enrolledCount >= course.Capacity) return BadRequest("Course is full.");
            var existing = await _enrollmentRepo.GetEnrollmentAsync(req.StudentId, req.CourseId);
            if (existing != null) return BadRequest("Already enrolled.");
            var enrollment = new Enrollment { StudentId = req.StudentId, CourseId = req.CourseId };
            await _enrollmentRepo.AddAsync(enrollment);
            await _enrollmentRepo.SaveAsync();
            return Ok("Enrolled successfully.");
        }

        // STUDENT: Unenroll from a course
        [HttpPost("unenroll")]
        public async Task<IActionResult> Unenroll([FromBody] EnrollmentRequest req)
        {
            var enrollment = await _enrollmentRepo.GetEnrollmentAsync(req.StudentId, req.CourseId);
            if (enrollment == null) return NotFound("Not enrolled.");
            _enrollmentRepo.Delete(enrollment);
            await _enrollmentRepo.SaveAsync();
            return Ok("Unenrolled successfully.");
        }

        // STUDENT: Get student's own courses
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentCourses(int studentId)
        {
            // Eagerly load Course and Teacher for each enrollment
            var enrollments = await _enrollmentRepo.GetAllAsync();
            var courseIds = enrollments.Where(e => e.StudentId == studentId).Select(e => e.CourseId).ToList();
            if (!courseIds.Any()) return Ok(new List<object>());
            var courses = await _courseRepo.GetCoursesWithTeacherAsync();
            var studentCourses = courses
                .Where(c => courseIds.Contains(c.Id))
                .Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    description = c.Description,
                    capacity = c.Capacity,
                    TeacherName = c.Teacher != null ? c.Teacher.Name : null
                })
                .ToList();
            return Ok(studentCourses);
        }
    }

    public class EnrollmentRequest
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
    }

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
