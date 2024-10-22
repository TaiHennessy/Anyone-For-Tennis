using AnyoneForTennis.Data;
using AnyoneForTennis.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AnyoneForTennis.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly LocalDbContext _context;

        public RegistrationController(UserManager<User> userManager, SignInManager<User> signInManager, LocalDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: /Registration/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View("~/Views/Home/Register.cshtml");
        }

        // POST: /Registration/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email is already in use
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                    return View("~/Views/Home/Register.cshtml", model);
                }

                // Create a new User instance
                var user = new User
                {
                    UserName = model.Username,
                    Email = model.Email,
                    IsAdmin = model.IsAdmin,
                    SecurityStamp = Guid.NewGuid().ToString() // Generate a new security stamp
                };

                // Create the user with hashed password
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Create a new Member instance
                    var member = new Member
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Active = true
                    };

                    _context.Member.Add(member);
                    await _context.SaveChangesAsync();

                    // Link the user and member in UserMember
                    var userMember = new UserMember
                    {
                        UserId = user.Id,
                        MemberId = member.MemberId
                    };

                    _context.UserMembers.Add(userMember);
                    await _context.SaveChangesAsync();

                    // Sign in the user after registration
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // Handle Identity errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we get this far, something failed
            return View("~/Views/Home/Register.cshtml", model);
        }


        // GET: /Registration/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View("~/Views/Home/Login.cshtml");
        }

        // POST: /Registration/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            Console.WriteLine("Login attempt for user: " + model.Username);

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve user by normalized username
                    var user = await _userManager.FindByNameAsync(model.Username.ToUpper());
                    if (user != null)
                    {
                        Console.WriteLine($"User found: {user.UserName} (ID: {user.Id})");

                        // Verify password
                        var result = await _signInManager.PasswordSignInAsync(
                            user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                        if (result.Succeeded)
                        {
                            Console.WriteLine($"Login successful for user: {user.UserName}");
                            return RedirectToAction("Index", "Home");
                        }
                        else if (result.IsLockedOut)
                        {
                            Console.WriteLine($"User {user.UserName} is locked out.");
                            ModelState.AddModelError(string.Empty, "Account locked. Try again later.");
                        }
                        else
                        {
                            Console.WriteLine($"Password mismatch for user: {user.UserName}");
                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"User not found: {model.Username}");
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    }
                }
                catch (Exception ex)
                {
                    // Log any exceptions that occur during the login process
                    Console.WriteLine($"Error during login: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again later.");
                }
            }
            else
            {
                Console.WriteLine("Model state is invalid. Check for missing or incorrect input.");
            }

            return View("~/Views/Home/Login.cshtml");
        }



        // POST: /Registration/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Logout", "Home"); // Redirect to Home/Logout after logging out
        }


    }
}
