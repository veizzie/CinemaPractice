using CinemaWeb.Models;
using CinemaWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinemaWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly CinemaDbContext _context;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, CinemaDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Неправильний логін або пароль");
            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var model = await LoadProfileDataAsync(user);

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                var refreshedModel = await LoadProfileDataAsync(user);
                return View("Profile", refreshedModel);
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Пароль успішно змінено.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            var modelWithErrors = await LoadProfileDataAsync(user);
            return View("Profile", modelWithErrors);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Email успішно підтверджено.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("Index", "Home");
        }

        private async Task<UserProfileViewModel> LoadProfileDataAsync(User user)
        {
            var allTickets = await _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Session).ThenInclude(s => s.Movie)
                .Include(t => t.Session).ThenInclude(s => s.Hall)
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.Session.StartTime)
                .ToListAsync();

            var model = new UserProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                IsEmailConfirmed = user.EmailConfirmed,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User",

                ActiveTickets = allTickets.Where(t => t.Session.StartTime > DateTime.Now).ToList(),
                HistoryTickets = allTickets.Where(t => t.Session.StartTime <= DateTime.Now).ToList()
            };

            return model;
        }
    }
}