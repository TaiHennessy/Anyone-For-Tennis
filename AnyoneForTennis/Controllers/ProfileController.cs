using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AnyoneForTennis.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using AnyoneForTennis.Data;

namespace AnyoneForTennis.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly LocalDbContext _context;

        public ProfileController(UserManager<User> userManager, SignInManager<User> signInManager, LocalDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // GET: Profile/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Registration");
            }

            // Retrieve the associated Member through UserMember relationship
            var userMember = await _context.UserMembers
                .Include(um => um.Member) // Ensure Member is included
                .FirstOrDefaultAsync(um => um.UserId == user.Id);

            var member = userMember?.Member;

            var model = new EditProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = member?.FirstName,
                LastName = member?.LastName
            };

            return View(model);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Registration");
            }

            // Retrieve the associated Member through UserMember relationship
            var userMember = await _context.UserMembers
                .Include(um => um.Member) // Ensure Member is included
                .FirstOrDefaultAsync(um => um.UserId == user.Id);

            var member = userMember?.Member;

            var model = new EditProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = member?.FirstName,
                LastName = member?.LastName
            };

            return View(model);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Registration");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Update user details
            user.UserName = model.UserName;
            user.Email = model.Email;

            // Retrieve the associated UserMember entry and include the Member entity
            var userMember = await _context.UserMembers
                .Include(um => um.Member)
                .FirstOrDefaultAsync(um => um.UserId == user.Id);

            if (userMember != null)
            {
                // Update the member's first and last name
                var member = userMember.Member;
                member.FirstName = model.FirstName;
                member.LastName = model.LastName;

                // Save the changes to the Member table
                _context.Update(member);
            }

            // Update user in the database
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Change password if provided
            if (!string.IsNullOrEmpty(model.OldPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            // Refresh sign-in to ensure the changes are reflected in the authentication cookie
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Index");
        }
    }
}
