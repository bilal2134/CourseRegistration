using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CourseRegistration.Data;
using CourseRegistration.Models;

namespace CourseRegistrationSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Utility function to check if the current user is an admin.
        private bool IsAdmin()
        {
            string isAdminString = HttpContext.Session.GetString("IsAdmin");
            return bool.TryParse(isAdminString, out bool isAdmin) && isAdmin;
        }

        // GET: /Admin/Courses
        public IActionResult Courses()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            var courses = _context.Courses.Include(c => c.CourseRegistrations).ToList();
            return View(courses);
        }

        // GET: /Admin/AddCourse
        public IActionResult AddCourse()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            return View();
        }

        // POST: /Admin/AddCourse
        [HttpPost]
        public IActionResult AddCourse(Course course)
        {
            if (!IsAdmin())
            {
                _logger.LogWarning("AddCourse was called by a non-admin user.");
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Courses.Add(course);
                    _context.SaveChanges();
                    _logger.LogInformation("Course added successfully with ID {CourseId}.", course.Id);
                    TempData["Message"] = "Course added successfully.";
                    return RedirectToAction("Courses");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while adding course.");
                    ModelState.AddModelError(string.Empty, "An error occurred while adding the course.");
                }
            }
            else
            {
                string errorMessages = string.Join(" | ",
                    ModelState.SelectMany(kvp => kvp.Value.Errors.Select(e => $"{kvp.Key}: {e.ErrorMessage}")));
                _logger.LogWarning("Model state is invalid. Errors: {Errors}", errorMessages);
            }
            return View(course);
        }


        // GET: /Admin/EditCourse/5
        public IActionResult EditCourse(int id)
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            var course = _context.Courses.Find(id);
            if (course == null)
            {
                return NotFound();
            }
            return View(course);
        }

        // POST: /Admin/EditCourse/5
        [HttpPost]
        public IActionResult EditCourse(Course course)
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            if (ModelState.IsValid)
            {
                _context.Courses.Update(course);
                _context.SaveChanges();
                TempData["Message"] = "Course updated successfully.";
                return RedirectToAction("Courses");
            }
            return View(course);
        }

        // POST: /Admin/DeleteCourse/5
        [HttpPost]
        public IActionResult DeleteCourse(int id)
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            var course = _context.Courses.Find(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                _context.SaveChanges();
                TempData["Message"] = "Course deleted successfully.";
            }
            return RedirectToAction("Courses");
        }

        // GET: /Admin/CourseRegistrations
        public IActionResult CourseRegistrations()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            // This view displays a list of courses along with the count of registered users.
            var coursesWithCount = _context.Courses
                                           .Include(c => c.CourseRegistrations)
                                           .Select(c => new
                                           {
                                               Course = c,
                                               RegistrationCount = c.CourseRegistrations.Count
                                           })
                                           .ToList();
            return View(coursesWithCount);
        }
    }
}
