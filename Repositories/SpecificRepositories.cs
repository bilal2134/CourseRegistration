using WebApplication1.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;


namespace WebApplication1.Repositories
{
    public interface ITeacherRepository : IGenericRepository<Teacher>
    {
        Task<Teacher> GetByEmailAsync(string email);
    }

    public interface IStudentRepository : IGenericRepository<Student>
    {
        Task<Student> GetByEmailAsync(string email);
        Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId);
    }

    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<IEnumerable<Course>> GetCoursesWithTeacherAsync();
        Task<IEnumerable<Student>> GetStudentsByCourseIdAsync(int courseId);
        Task<IEnumerable<Course>> GetCoursesByTeacherIdAsync(int teacherId);
    }

    public interface IEnrollmentRepository : IGenericRepository<Enrollment>
    {
        Task<Enrollment> GetEnrollmentAsync(int studentId, int courseId);
        Task<int> GetEnrollmentCountByCourseIdAsync(int courseId);
    }
}


namespace WebApplication1.Repositories
{
    public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Teacher> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.Email == email);
        }
    }

    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Student> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.Email == email);
        }

        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
        {
            return await _context.Enrollments.Where(e => e.StudentId == studentId).ToListAsync();
        }
    }

    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Course>> GetCoursesWithTeacherAsync()
        {
            return await _dbSet.Include(c => c.Teacher).ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByCourseIdAsync(int courseId)
        {
            return await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Select(e => e.Student)
                .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByTeacherIdAsync(int teacherId)
        {
            return await _context.Courses.Where(c => c.TeacherId == teacherId).ToListAsync();
        }
    }

    public class EnrollmentRepository : GenericRepository<Enrollment>, IEnrollmentRepository
    {
        public EnrollmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Enrollment> GetEnrollmentAsync(int studentId, int courseId)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);
        }

        public async Task<int> GetEnrollmentCountByCourseIdAsync(int courseId)
        {
            return await _dbSet.CountAsync(e => e.CourseId == courseId);
        }
    }
}
