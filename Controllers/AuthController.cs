using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Repositories;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITeacherRepository _teacherRepo;
        private readonly IStudentRepository _studentRepo;

        public AuthController(ITeacherRepository teacherRepo, IStudentRepository studentRepo)
        {
            _teacherRepo = teacherRepo;
            _studentRepo = studentRepo;
        }

        [HttpPost("register/teacher")]
        public async Task<IActionResult> RegisterTeacher([FromBody] Teacher teacher)
        {
            var existing = await _teacherRepo.GetByEmailAsync(teacher.Email);
            if (existing != null)
                return BadRequest("Email already registered.");
            await _teacherRepo.AddAsync(teacher);
            await _teacherRepo.SaveAsync();
            return Ok("Teacher registered successfully.");
        }

        [HttpPost("register/student")]
        public async Task<IActionResult> RegisterStudent([FromBody] Student student)
        {
            var existing = await _studentRepo.GetByEmailAsync(student.Email);
            if (existing != null)
                return BadRequest("Email already registered.");
            await _studentRepo.AddAsync(student);
            await _studentRepo.SaveAsync();
            return Ok("Student registered successfully.");
        }

        [HttpPost("login/teacher")]
        public async Task<IActionResult> LoginTeacher([FromBody] LoginRequest req)
        {
            var teacher = await _teacherRepo.GetByEmailAsync(req.Email);
            if (teacher == null || teacher.Password != req.Password)
                return Unauthorized("Invalid credentials.");
            return Ok(new { teacher.Id, teacher.Name, teacher.Email });
        }

        [HttpPost("login/student")]
        public async Task<IActionResult> LoginStudent([FromBody] LoginRequest req)
        {
            var student = await _studentRepo.GetByEmailAsync(req.Email);
            if (student == null || student.Password != req.Password)
                return Unauthorized("Invalid credentials.");
            return Ok(new { student.Id, student.Name, student.Email });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
