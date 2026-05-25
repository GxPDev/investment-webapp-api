using Microsoft.AspNetCore.Mvc;
using InvestmentWebApp.Data;
using InvestmentWebApp.Models;
using InvestmentWebApp.Services;
using System.Linq;
using System;

namespace InvestmentWebApp.Controllers
{
    public class AuthController : Controller
    {
        private const string AuthUserSessionKey = "user";
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public AuthController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Login() => RedirectToAction("Index", "Home");

        public IActionResult Register() => RedirectToAction("Index", "Home");

        [HttpPost]
        public IActionResult Register(AppUser user, string returnMode)
        {
            user.Username = user.Username?.Trim().ToLowerInvariant() ?? string.Empty;
            user.Email = user.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 7)
            {
                return HandleRegisterError("Password must be at least 7 characters.", returnMode);
            }

            if (string.IsNullOrWhiteSpace(user.Username))
            {
                return HandleRegisterError("Username is required.", returnMode);
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return HandleRegisterError("Email address is required.", returnMode);
            }

            if (_context.Users.Any(u => u.Username.ToLower() == user.Username))
            {
                return HandleRegisterError("Username already exists. Please choose another.", returnMode);
            }

            if (_context.Users.Any(u => u.Email.ToLower() == user.Email))
            {
                return HandleRegisterError("Email address already exists. Please use another.", returnMode);
            }

            user.Tier = "Bronze";
            user.Balance = 100;
            if (user.Username == "admin")
                user.Role = "Admin";
            else
                user.Role = "User";

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                _emailService.SendEmail(
                    user.Email,
                    "Welcome to Investment Web App",
                    $"Hello {user.Username},\n\nThank you for registering with Investment Web App. Your account has been created successfully.\n\nNext, please sign in to start managing your investments.\n\nBest regards,\nInvestment Web App team");
            }

            TempData["RegisterSuccess"] = "Account registration successful. Please sign in.";
            if (string.Equals(returnMode, "modal", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Login(string username, string password, string returnMode)
        {
            var normalizedUsername = username?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrWhiteSpace(password))
            {
                return HandleLoginError("Login details do not match.", returnMode);
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Username == normalizedUsername);

            if (user == null)
            {
                return HandleLoginError("Login details do not match.", returnMode);
            }

            bool validPassword;

            try
            {
                validPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                validPassword = user.Password == password;

                if (validPassword)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                    _context.SaveChanges();
                }
            }

            if (!validPassword)
            {
                return HandleLoginError("Login details do not match.", returnMode);
            }

            HttpContext.Session.SetString(AuthUserSessionKey, normalizedUsername);
            HttpContext.Session.SetString("role", user.Role);

            return RedirectToAction("Index", "Dashboard");
        }

        private IActionResult HandleLoginError(string message, string returnMode)
        {
            TempData["LoginError"] = message;
            return RedirectToAction("Index", "Home");
        }

        private IActionResult HandleRegisterError(string message, string returnMode)
        {
            TempData["RegisterError"] = message;
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove(AuthUserSessionKey);
            TempData["LogoutMessage"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }
    }
}
