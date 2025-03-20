using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CourseRegistration.Data;
using CourseRegistration.Models;

namespace CourseRegistrationSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and Password are required.");
                return View();
            }

            // Check if user exists
            if (_context.Users.Any(u => u.Email == email))
            {
                ModelState.AddModelError("", "User already exists.");
                return View();
            }

            var user = new User
            {
                Email = email,
                PasswordHash = ComputeHash(password),
                IsAdmin = false // default to normal user; set manually if needed
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Redirect to login page after successful registration
            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var passwordHash = ComputeHash(password);
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == passwordHash);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View();
            }

            // Store user info in session (for demo purposes)
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Utility function to compute SHA256 hash for the password
        private string ComputeHash(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
