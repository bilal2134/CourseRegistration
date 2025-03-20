using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using CourseRegistration.Data;

namespace CourseRegistrationSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CourseController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Course/List?searchTerm=keyword
        public IActionResult List(string searchTerm = null)
        {
            var courses = string.IsNullOrWhiteSpace(searchTerm)
                ? _context.Courses.ToList()
                : _context.Courses.Where(c => c.Name.Contains(searchTerm)).ToList();
            return View(courses);
        }

        // GET: /Course/Details/5
        public IActionResult Details(int id)
        {
            var course = _context.Courses
                                 .Include(c => c.CourseRegistrations)
                                 .ThenInclude(cr => cr.User)
                                 .FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: /Course/Register
        [HttpPost]
        public IActionResult Register(int courseId)
        {
            // Ensure user is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if already registered for the course
            bool alreadyRegistered = _context.CourseRegistrations.Any(cr => cr.UserId == userId && cr.CourseId == courseId);
            if (alreadyRegistered)
            {
                TempData["Error"] = "Already registered for this course.";
                return RedirectToAction("Details", new { id = courseId });
            }

            var registration = new CourseRegistration.Models.CourseRegistration
            {
                UserId = userId.Value,
                CourseId = courseId
            };
            _context.CourseRegistrations.Add(registration);
            _context.SaveChanges();

            TempData["Message"] = "Successfully registered for the course.";
            return RedirectToAction("Details", new { id = courseId });
        }

        // POST: /Course/Unregister
        [HttpPost]
        public IActionResult Unregister(int courseId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var registration = _context.CourseRegistrations.FirstOrDefault(cr => cr.UserId == userId && cr.CourseId == courseId);
            if (registration != null)
            {
                _context.CourseRegistrations.Remove(registration);
                _context.SaveChanges();
                TempData["Message"] = "Successfully unregistered from the course.";
            }
            else
            {
                TempData["Error"] = "You are not registered for this course.";
            }
            return RedirectToAction("Details", new { id = courseId });
        }

        // GET: /Course/MyCourses
        public IActionResult MyCourses()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var myCourses = _context.CourseRegistrations
                                    .Include(cr => cr.Course)
                                    .Where(cr => cr.UserId == userId)
                                    .Select(cr => cr.Course)
                                    .ToList();
            return View(myCourses);
        }
    }
}
