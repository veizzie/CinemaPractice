using CinemaWeb.Models;
using CinemaWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                // Додаємо користувача (пароль хешується сам)
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");

                    // Одразу вхід в систему після реєстрації
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
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
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Передача у View дані користувача
            ViewBag.UserEmail = user.Email;
            ViewBag.UserFullName = user.FullName;
            ViewBag.IsEmailConfirmed = user.EmailConfirmed;

            return View(new ChangePasswordViewModel());
        }

        // При створенні AspNet таблиць в базі, з'явилось поле підтвердження email, тому я вирішив додати цю функціональність але просто як галочка
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null) return RedirectToAction("Login");

            // Просто ставлю галочку що email підтверджено
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            // Оновлюю сесію, щоб одразу застосувати зміни
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Email успішно підтверджено.";
            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // При помилці валідації - повернення на ту ж сторінку
                var user = await _userManager.GetUserAsync(User);
                ViewBag.UserEmail = user?.Email;
                ViewBag.UserFullName = user?.FullName;
                return View("Profile", model);
            }

            var userCurrent = await _userManager.GetUserAsync(User);
            if (userCurrent == null)
            {
                return RedirectToAction("Login");
            }

            // Спроба зміни пароля
            var result = await _userManager.ChangePasswordAsync(userCurrent, model.CurrentPassword, model.NewPassword);
            
            if(result.Succeeded)
            {
                // Щоб не викинуло з аккаунту після зміни пароля, оновлюється кукі
                await _signInManager.RefreshSignInAsync(userCurrent);

                TempData["SuccessMessage"] = "Пароль успішно змінено.";
                return RedirectToAction("Profile");
            }

            // При про милці
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.UserEmail = userCurrent.Email;
            ViewBag.UserFullName = userCurrent.FullName;
            return View("Profile", model);
        }

        [HttpPost] // Для виходу тільки пост-запит
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
    }
}
