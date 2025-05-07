using Microsoft.AspNetCore.Mvc;
using WebApplication1.Repositories;
using System.Threading.Tasks;
using System.Linq;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ITeacherRepository _teacherRepo;
        private readonly IStudentRepository _studentRepo;
        private const string AdminUsername = "admin";
        private const string AdminPassword = "admin123"; // Change as needed

        public AdminController(ITeacherRepository teacherRepo, IStudentRepository studentRepo)
        {
            _teacherRepo = teacherRepo;
            _studentRepo = studentRepo;
        }

        [HttpPost("login")]
        public IActionResult AdminLogin([FromBody] AdminLoginRequest req)
        {
            if (req.Username == AdminUsername && req.Password == AdminPassword)
                return Ok("Admin login successful.");
            return Unauthorized("Invalid admin credentials.");
        }

        [HttpDelete("delete/teacher/{id}")]
        public async Task<IActionResult> DeleteTeacher(int id, [FromQuery] string adminUsername, [FromQuery] string adminPassword)
        {
            if (adminUsername != AdminUsername || adminPassword != AdminPassword)
                return Unauthorized("Invalid admin credentials.");
            var teacher = await _teacherRepo.GetByIdAsync(id);
            if (teacher == null) return NotFound("Teacher not found.");
            _teacherRepo.Delete(teacher);
            await _teacherRepo.SaveAsync();
            return Ok("Teacher deleted.");
        }

        [HttpDelete("delete/student/{id}")]
        public async Task<IActionResult> DeleteStudent(int id, [FromQuery] string adminUsername, [FromQuery] string adminPassword)
        {
            if (adminUsername != AdminUsername || adminPassword != AdminPassword)
                return Unauthorized("Invalid admin credentials.");
            var student = await _studentRepo.GetByIdAsync(id);
            if (student == null) return NotFound("Student not found.");
            _studentRepo.Delete(student);
            await _studentRepo.SaveAsync();
            return Ok("Student deleted.");
        }

        // ADMIN: List all teachers
        [HttpGet("teachers")]
        public async Task<IActionResult> GetAllTeachers([FromQuery] string adminUsername, [FromQuery] string adminPassword)
        {
            if (adminUsername != AdminUsername || adminPassword != AdminPassword)
                return Unauthorized("Invalid admin credentials.");
            var teachers = await _teacherRepo.GetAllAsync();
            var result = teachers.Select(t => new { t.Id, t.Name, t.Email });
            return Ok(result);
        }

        // ADMIN: List all students
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents([FromQuery] string adminUsername, [FromQuery] string adminPassword)
        {
            if (adminUsername != AdminUsername || adminPassword != AdminPassword)
                return Unauthorized("Invalid admin credentials.");
            var students = await _studentRepo.GetAllAsync();
            var result = students.Select(s => new { s.Id, s.Name, s.Email });
            return Ok(result);
        }
    }

    public class AdminLoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
